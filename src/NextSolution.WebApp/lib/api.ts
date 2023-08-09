import axios, {
  AxiosError,
  AxiosInstance,
  AxiosRequestConfig,
  AxiosResponse,
  CreateAxiosDefaults,
  HttpStatusCode,
  isAxiosError
} from "axios";
import QueryString from "qs";
import { BehaviorSubject } from "rxjs";

import { ExternalWindow } from "./external-window";

export type UserCredentials = {
  username: string;
  password: string;
};

export type UserSession = {
  accessToken: string;
  refreshToken: string;
  [key: string]: any;
};

export interface ApiConfig extends CreateAxiosDefaults {
  initialUser?: UserSession;
}

export class Api {
  private axiosInstance: AxiosInstance;
  public userSubject: BehaviorSubject<UserSession | undefined | null>;

  private refreshing: boolean;
  private retryRequests: Array<() => void>;

  constructor(config?: ApiConfig) {
    const defaultConfig = {
      paramsSerializer: {
        encode: (params) => QueryString.stringify(params, { arrayFormat: "brackets" })
      },
      withCredentials: true
    } as ApiConfig;

    const { initialUser, ...axiosConfig } = { ...defaultConfig, ...config };

    this.axiosInstance = axios.create(axiosConfig);
    this.userSubject = new BehaviorSubject<UserSession | undefined | null>(initialUser);

    this.refreshing = false;
    this.retryRequests = [];

    // Add request interceptor
    this.axiosInstance.interceptors.request.use(
      (requestConfig) => {
        const existingUser = this.userSubject.getValue();

        if (existingUser) {
          requestConfig.headers.setAuthorization(`Bearer ${existingUser.accessToken}`);
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

          const existingUser = this.userSubject.getValue();
          if (!existingUser) {
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
          return this.refresh(existingUser.refreshToken)
            .then(({ data: user }) => {
              this.userSubject.next(user);
              this.refreshing = false;
              this.retryRequests.forEach((prom) => prom());
              this.retryRequests = [];
              return this.axiosInstance.request(originalRequest);
            })
            .catch(() => {
              this.userSubject.next(null);
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

  public async signIn<T extends UserSession, R extends AxiosResponse<T>, D extends any>(
    credentials: UserCredentials,
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/users/sessions`,
      method: "POST",
      data: credentials,
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.userSubject.next(response.data);
    return response;
  }

  public async signInWith<T extends UserSession, R extends AxiosResponse<T>, D extends any>(
    provider: string,
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/users/sessions/${provider}`,
      method: "POST",
      ...config
    } as AxiosRequestConfig<D>;

    const externalURL = new URL(`${this.axiosInstance.defaults.baseURL}/users/sessions/${provider}`);
    externalURL.searchParams.set("returnUrl", window.origin);

    try {
      await ExternalWindow.open(externalURL.toString(), { center: true });
    } catch {}

    const response = await this.axiosInstance.request<T, R, D>(config);
    this.userSubject.next(response.data);
    return response;
  }

  public async refresh<T extends UserSession, R extends AxiosResponse<T>, D extends any>(
    refreshToken: string,
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/users/sessions/refresh`,
      method: "POST",
      data: { refreshToken },
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.userSubject.next(response.data);
    return response;;
  }

  public async signOut<T extends any, R extends AxiosResponse<T>, D extends any>(config?: AxiosRequestConfig<D>): Promise<R> {
    config = {
      url: `/users/sessions/revoke`,
      method: "POST",
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.userSubject.next(null);
    return response;;
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

  public patch<T extends any, R extends AxiosResponse<T>, D extends any>(
    url: string,
    data?: D,
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    return this.axiosInstance.patch<T, R, D>(url, data, config);
  }
}
