import axios, { AxiosError, AxiosInstance, AxiosRequestConfig, AxiosResponse, CreateAxiosDefaults, HttpStatusCode, isAxiosError } from "axios";
import { createState, State } from "state-pool";

import { ExternalWindow } from "../external-window";
import { parseJSON, stringifyJSON } from "../utils";
import { ApiConfig, ApiState, ApiStore, User } from "./types";

const API_STATE_KEY = "API_STATE";

export class Api {
  private axiosInstance: AxiosInstance;
  public config: ApiConfig;
  private store: ApiStore;
  public state: State<ApiState>;

  private refreshing: boolean;
  private retryRequests: Array<() => void>;

  constructor(config: ApiConfig) {
    const defaultAxiosConfig = {} as CreateAxiosDefaults;

    const { store, ...axiosConfig } = (config = { ...defaultAxiosConfig, ...config });

    this.axiosInstance = axios.create(axiosConfig);
    this.config = config;
    this.store = store;
    this.state = createState(parseJSON(store.get(API_STATE_KEY)) ?? {});
    this.state.subscribe<ApiState>((state) => {
      this.store.set(API_STATE_KEY, stringifyJSON(state));
    });

    this.refreshing = false;
    this.retryRequests = [];

    // Add request interceptor
    this.axiosInstance.interceptors.request.use(
      (requestConfig) => {
        const currentState = this.state.getValue<ApiState>();

        if (currentState.user) {
          requestConfig.headers.setAuthorization(`${currentState.user.tokenType} ${currentState.user.accessToken}`);
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
        console.log("Response Interceptor:", response);
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

          const currentUser = this.state.getValue<ApiState>().user;
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
            .then(({ data: user }) => {
              this.state.updateValue((currentState) => (currentState.user = user));
              this.refreshing = false;
              this.retryRequests.forEach((prom) => prom());
              this.retryRequests = [];
              return this.axiosInstance.request(originalRequest);
            })
            .catch(() => {
              this.state.updateValue((currentState) => (currentState.user = null));
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

  public async signUp<T extends User, R extends AxiosResponse<T>, D extends any>(
    data: { firstName: string; lastName: string; username: string; password: string; [key: string]: any },
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/users/register`,
      method: "POST",
      data,
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.state.updateValue((currentState) => (currentState.user = response.data));
    return response;
  }

  public async signIn<T extends User, R extends AxiosResponse<T>, D extends any>(
    data: { username: string; password: string; [key: string]: any },
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/users/session/generate`,
      method: "POST",
      data,
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.state.updateValue((currentState) => (currentState.user = response.data));
    return response;
  }

  public async signInWith<T extends User, R extends AxiosResponse<T>, D extends any>(provider: string, config?: AxiosRequestConfig<D>): Promise<R> {
    config = {
      url: `/users/${provider}/session/generate`,
      method: "POST",
      ...config
    } as AxiosRequestConfig<D>;

    try {
      const externalUrl = new URL(`${this.axiosInstance.defaults.baseURL}/users/${provider}/session/generate`);
      externalUrl.searchParams.set("returnUrl", window.location.href);
      await ExternalWindow.open(externalUrl, { center: true });
    } catch (error) {
      console.warn(error);
    }
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.state.updateValue((currentState) => (currentState.user = response.data));
    return response;
  }

  public async refresh<T extends User, R extends AxiosResponse<T>, D extends any>(config?: AxiosRequestConfig<D>): Promise<R> {
    const currentUser = this.state.getValue<ApiState>().user;
    const data = currentUser != null ? { userId: currentUser.id, refreshToken: currentUser.refreshToken } : {};

    config = {
      url: `/users/session/refresh`,
      method: "POST",
      data: data,
      ...config
    } as AxiosRequestConfig<D>;

    try {
      const response = await this.axiosInstance.request<T, R, D>(config);
      this.state.updateValue((currentState) => (currentState.user = response.data));
      return response;
    } catch (error) {
      if (isAxiosError(error) && error.response && (error.response.status == HttpStatusCode.BadRequest || error.response.status == HttpStatusCode.Unauthorized)) {
        this.state.updateValue((currentState) => (currentState.user = null));
      }
      throw error;
    }
  }

  public async signOut<T extends any, R extends AxiosResponse<T>, D extends any>(config?: AxiosRequestConfig<D>): Promise<R> {
    config = {
      url: `/users/session/revoke`,
      method: "POST",
      data: { refreshToken: this.state.getValue<ApiState>().user?.refreshToken },
      ...config
    } as AxiosRequestConfig<D>;
    return this.axiosInstance.request<T, R, D>(config).finally(() => {
      this.state.updateValue((currentState) => (currentState.user = null));
    });
  }

  public async sendResetPasswordCode<T extends User, R extends AxiosResponse<T>, D extends any>(
    data: { username: string; [key: string]: any },
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/users/password/reset/send-code`,
      method: "POST",
      data,
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    return response;
  }

  public async resetPassword<T extends User, R extends AxiosResponse<T>, D extends any>(
    data: { username: string; code: string; password: string; [key: string]: any },
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/users/password/reset`,
      method: "POST",
      data,
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.state.updateValue((currentState) => (currentState.user = response.data));
    return response;
  }

  public async sendUsernameVerifyCode<T extends User, R extends AxiosResponse<T>, D extends any>(
    data: { username: string; usernameType: string; [key: string]: any },
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/users/username/verify/send-code`,
      method: "POST",
      data,
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    return response;
  }

  public async verifyUsername<T extends User, R extends AxiosResponse<T>, D extends any>(
    data: { username: string; usernameType: string; code: string; [key: string]: any },
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/users/username/verify`,
      method: "POST",
      data,
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.state.updateValue((currentState) => (currentState.user = response.data));
    return response;
  }

  public request<T extends any, R extends AxiosResponse<T>, D extends any>(config: AxiosRequestConfig<D>): Promise<R> {
    return this.axiosInstance.request<T, R, D>(config);
  }

  public get<T extends any, R extends AxiosResponse<T>, D extends any>(url: string, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.axiosInstance.get<T, R, D>(url, config);
  }

  public delete<T extends any, R extends AxiosResponse<T>, D extends any>(url: string, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.axiosInstance.delete<T, R, D>(url, config);
  }

  public head<T extends any, R extends AxiosResponse<T>, D extends any>(url: string, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.axiosInstance.head<T, R, D>(url, config);
  }

  public options<T extends any, R extends AxiosResponse<T>, D extends any>(url: string, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.axiosInstance.options<T, R, D>(url, config);
  }

  public post<T extends any, R extends AxiosResponse<T>, D extends any>(url: string, data?: D, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.axiosInstance.post<T, R, D>(url, data, config);
  }

  public put<T extends any, R extends AxiosResponse<T>, D extends any>(url: string, data?: D, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.axiosInstance.put<T, R, D>(url, data, config);
  }

  public patch<T extends any, R extends AxiosResponse<T>, D extends any>(url: string, data?: D, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.axiosInstance.patch<T, R, D>(url, data, config);
  }
}