import axios, { AxiosError, AxiosInstance, AxiosRequestConfig, AxiosResponse, CreateAxiosDefaults, HttpStatusCode, isAxiosError } from "axios";
import { BehaviorSubject } from "rxjs";

import { ExternalWindow } from "../external-window";
import { decryptData, encryptData } from "../utils";
import { ApiConfig, ApiStore, User } from "./types";

const CURRENT_USER_STORE_NAME = "CURRENT_USER";
const CURRENT_USER_STORE_KEY = "Hn411*x,Y1vB,K}\u00A3'I<g<u\":&[9Uu7;Y\u00A3:ns|.Q2?e#:1!_8Db";

export class Api {
  private axiosInstance: AxiosInstance;
  public config: ApiConfig;
  public user: BehaviorSubject<User | null | undefined>;
  private store: ApiStore;

  private refreshing: boolean;
  private retryRequests: Array<() => void>;

  constructor(config: ApiConfig) {
    const defaultAxiosConfig = {} as CreateAxiosDefaults;

    const { store, ...axiosConfig } = (config = { ...defaultAxiosConfig, ...config });

    this.axiosInstance = axios.create(axiosConfig);
    this.config = config;
    this.store = store;
    this.user = new BehaviorSubject(decryptData(store.get(CURRENT_USER_STORE_NAME), CURRENT_USER_STORE_KEY, false));
    this.user.subscribe((currentUser) => {
      this.store.set(CURRENT_USER_STORE_NAME, encryptData(currentUser, CURRENT_USER_STORE_KEY, false));
    });

    this.refreshing = false;
    this.retryRequests = [];

    // Add request interceptor
    this.axiosInstance.interceptors.request.use(
      (requestConfig) => {
        const currentUser = this.user.getValue();

        if (currentUser) {
          requestConfig.headers.setAuthorization(`${currentUser.tokenType} ${currentUser.accessToken}`);
        } else {
          delete requestConfig.headers.Authorization;
        }

        return requestConfig;
      },
      (error: AxiosError | Error) => {
        // Handle request errors if needed
        console.error("Request Interceptor Error:", error);
        return Promise.reject(error);
      }
    );

    // Add response interceptor
    this.axiosInstance.interceptors.response.use(
      (response: AxiosResponse) => {
        // You can perform any preprocessing of the response here
        // For example, handling errors, transforming data, etc.
        return response;
      },
      (error: AxiosError | Error) => {
        if (isAxiosError(error) && error.response) {
          const { config, response } = error;
          const originalRequest = config as AxiosRequestConfig & { retryCount: number };

          if (response.status != HttpStatusCode.Unauthorized) {
            return Promise.reject(error);
          }

          originalRequest.retryCount = (originalRequest.retryCount ?? 0) + 1;
          if (originalRequest.retryCount > 2) {
            // If already retried twice, reject the request
            return Promise.reject(error);
          }

          const currentUser = this.user.getValue();
          if (!currentUser) {
            return Promise.reject(error);
          }

          if (this.refreshing) {
            // If already refreshing, add the failed request to the queue
            const retryOriginalRequest = new Promise<AxiosResponse>((resolve) => {
              this.retryRequests.push(() => resolve(this.axiosInstance.request(originalRequest!)));
            });
            return retryOriginalRequest;
          }

          this.refreshing = true;
          return this.refresh()
            .then(({ data: value }) => {
              this.user.next(value);
              this.refreshing = false;
              this.retryRequests.forEach((prom) => prom());
              this.retryRequests = [];
              return this.axiosInstance.request(originalRequest);
            })
            .catch(() => {
              this.user.next(null);
              this.refreshing = false;
              this.retryRequests.forEach((prom) => prom());
              this.retryRequests = [];
              throw error;
            });
        }

        return Promise.reject(error);
      }
    );
  }

  public async signUp<T extends User = User, R extends AxiosResponse<T> = AxiosResponse<T>, D = { username: string; password: string; [key: string]: any }>(
    data: D,
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/users/register`,
      method: "POST",
      data,
      ...config
    };
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.user.next(response.data);
    return response;
  }

  public async signIn<T extends User = User, R extends AxiosResponse<T> = AxiosResponse<T>, D = { username: string; password: string; [key: string]: any }>(
    data: D,
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/users/session/generate`,
      method: "POST",
      data,
      ...config
    };
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.user.next(response.data);
    return response;
  }

  public async signInWith<T extends User = User, R extends AxiosResponse<T> = AxiosResponse<T>, D = any>(provider: string, config?: AxiosRequestConfig<D>): Promise<R> {
    config = {
      url: `/users/${provider}/session/generate`,
      method: "POST",
      ...config
    };

    try {
      const externalUrl = new URL(`${this.axiosInstance.defaults.baseURL}/users/${provider}/session/generate`);
      externalUrl.searchParams.set("returnUrl", window.location.href);
      await ExternalWindow.open(externalUrl, { center: true });
    } catch (error) {
      console.warn(error);
    }
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.user.next(response.data);
    return response;
  }

  public async refresh<T extends User = User, R extends AxiosResponse<T> = AxiosResponse<T>, D = any>(config?: AxiosRequestConfig<D>): Promise<R> {
    const currentUser = this.user.getValue();
    const data = (currentUser != null ? { userId: currentUser.id, refreshToken: currentUser.refreshToken } : {}) as D;

    config = {
      url: `/users/session/refresh`,
      method: "POST",
      data,
      ...config
    };

    try {
      const response = await this.axiosInstance.request<T, R, D>(config);
      this.user.next(response.data);
      return response;
    } catch (error) {
      if (isAxiosError(error) && error.response && (error.response.status == HttpStatusCode.BadRequest || error.response.status == HttpStatusCode.Unauthorized)) {
        this.user.next(null);
      }
      throw error;
    }
  }

  public async signOut<T = any, R = AxiosResponse<T>, D = any>(config?: AxiosRequestConfig<D>): Promise<R> {
    const currentUser = this.user.getValue();
    const data = (currentUser != null ? { userId: currentUser.id, refreshToken: currentUser.refreshToken } : {}) as D;

    config = {
      url: `/users/session/revoke`,
      method: "POST",
      data,
      ...config
    };
    return this.axiosInstance.request<T, R, D>(config).finally(() => {
      this.user.next(null);
    });
  }

  public async sendResetPasswordCode<T extends User = User, R extends AxiosResponse<T> = AxiosResponse<T>, D = any>(data: D, config?: AxiosRequestConfig<D>): Promise<R> {
    config = {
      url: `/users/password/reset/send-code`,
      method: "POST",
      data,
      ...config
    };
    const response = await this.axiosInstance.request<T, R, D>(config);
    return response;
  }

  public async resetPassword<T extends User = User, R extends AxiosResponse<T> = AxiosResponse<T>, D = any>(data: D, config?: AxiosRequestConfig<D>): Promise<R> {
    config = {
      url: `/users/password/reset`,
      method: "POST",
      data,
      ...config
    };
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.user.next(response.data);
    return response;
  }

  public async sendUsernameVerifyCode<T extends User = User, R extends AxiosResponse<T> = AxiosResponse<T>, D = any>(data: D, config?: AxiosRequestConfig<D>): Promise<R> {
    config = {
      url: `/users/username/verify/send-code`,
      method: "POST",
      data,
      ...config
    };
    const response = await this.axiosInstance.request<T, R, D>(config);
    return response;
  }

  public async verifyUsername<T extends User = User, R extends AxiosResponse<T> = AxiosResponse<T>, D = any>(data: D, config?: AxiosRequestConfig<D>): Promise<R> {
    config = {
      url: `/users/username/verify`,
      method: "POST",
      data,
      ...config
    };
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.user.next(response.data);
    return response;
  }

  public request<T = any, R = AxiosResponse<T>, D = any>(config: AxiosRequestConfig<D>): Promise<R> {
    return this.axiosInstance.request<T, R, D>(config);
  }

  public get<T = any, R = AxiosResponse<T>, D = any>(url: string, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.axiosInstance.get<T, R, D>(url, config);
  }

  public delete<T = any, R = AxiosResponse<T>, D = any>(url: string, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.axiosInstance.delete<T, R, D>(url, config);
  }

  public head<T = any, R = AxiosResponse<T>, D = any>(url: string, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.axiosInstance.head<T, R, D>(url, config);
  }

  public options<T = any, R = AxiosResponse<T>, D = any>(url: string, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.axiosInstance.options<T, R, D>(url, config);
  }

  public post<T = any, R = AxiosResponse<T>, D = any>(url: string, data?: D, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.axiosInstance.post<T, R, D>(url, data, config);
  }

  public put<T = any, R = AxiosResponse<T>, D = any>(url: string, data?: D, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.axiosInstance.put<T, R, D>(url, data, config);
  }

  public patch<T = any, R = AxiosResponse<T>, D = any>(url: string, data?: D, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.axiosInstance.patch<T, R, D>(url, data, config);
  }
}
