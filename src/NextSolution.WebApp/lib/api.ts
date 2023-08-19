import axios, {
  AxiosError,
  AxiosInstance,
  AxiosRequestConfig,
  AxiosResponse,
  CreateAxiosDefaults,
  HttpStatusCode,
  isAxiosError
} from "axios";
import { isIdempotentRequestError, isNetworkError } from "axios-retry";
import QueryString from "query-string";
import { BehaviorSubject } from "rxjs";

import { ExternalWindow } from "./external-window";

export type ApiTokens = {
  tokenType: string;
  accessToken: string;
  refreshToken: string;
  [key: string]: any;
};

export interface ApiConfig extends CreateAxiosDefaults {
  initialUser?: ApiTokens;
}

export class Api {
  private axiosInstance: AxiosInstance;
  public tokenStore: BehaviorSubject<ApiTokens | undefined | null>;

  private refreshing: boolean;
  private retryRequests: Array<() => void>;

  constructor(config?: ApiConfig) {
    const defaultConfig = {
      paramsSerializer: {
        encode: (params) => QueryString.stringify(params, { arrayFormat: "bracket" })
      },
      withCredentials: true
    } as ApiConfig;

    const { initialUser, ...axiosConfig } = { ...defaultConfig, ...config };

    this.axiosInstance = axios.create(axiosConfig);
    this.tokenStore = new BehaviorSubject<ApiTokens | undefined | null>(initialUser);

    this.refreshing = false;
    this.retryRequests = [];

    // Add request interceptor
    this.axiosInstance.interceptors.request.use(
      (requestConfig) => {
        const existingUser = this.tokenStore.getValue();

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

          const existingUser = this.tokenStore.getValue();
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
              this.tokenStore.next(user);
              this.refreshing = false;
              this.retryRequests.forEach((prom) => prom());
              this.retryRequests = [];
              return this.axiosInstance.request(originalRequest);
            })
            .catch(() => {
              this.tokenStore.next(null);
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

  public async signUp<T extends ApiTokens, R extends AxiosResponse<T>, D extends any>(
    data: { firstName: string; lastName: string; username: string; password: string; [key: string]: any },
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/accounts`,
      method: "POST",
      data,
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.tokenStore.next(response.data);
    return response;
  }

  public async signIn<T extends ApiTokens, R extends AxiosResponse<T>, D extends any>(
    data: { username: string; password: string; [key: string]: any },
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/accounts/sessions`,
      method: "POST",
      data,
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.tokenStore.next(response.data);
    return response;
  }

  public async signInWith<T extends ApiTokens, R extends AxiosResponse<T>, D extends any>(
    provider: string,
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/accounts/sessions/${provider}`,
      method: "POST",
      ...config
    } as AxiosRequestConfig<D>;

    try {
      const externalUrl = new URL(`${this.axiosInstance.defaults.baseURL}/accounts/sessions/${provider}`);
      externalUrl.searchParams.set("returnUrl", window.location.href);
      await ExternalWindow.open(externalUrl, { center: true });
    } catch (error) {
      console.warn(error);
    }
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.tokenStore.next(response.data);
    return response;
  }

  public async refresh<T extends ApiTokens, R extends AxiosResponse<T>, D extends any>(
    refreshToken: string,
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/accounts/sessions/refresh`,
      method: "POST",
      data: { refreshToken },
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.tokenStore.next(response.data);
    return response;
  }

  public async signOut<T extends any, R extends AxiosResponse<T>, D extends any>(config?: AxiosRequestConfig<D>): Promise<R> {
    config = {
      url: `/accounts/sessions/revoke`,
      method: "POST",
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.tokenStore.next(null);
    return response;
  }

  public async resetPasswordCode<T extends ApiTokens, R extends AxiosResponse<T>, D extends any>(
    data: { username: string; [key: string]: any },
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/accounts/password/reset/send-code`,
      method: "POST",
      data,
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.tokenStore.next(response.data);
    return response;
  }

  public async resetPassword<T extends ApiTokens, R extends AxiosResponse<T>, D extends any>(
    data: { username: string; code: string; password: string; [key: string]: any },
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/accounts/password/reset`,
      method: "POST",
      data,
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.tokenStore.next(response.data);
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

  public patch<T extends any, R extends AxiosResponse<T>, D extends any>(
    url: string,
    data?: D,
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    return this.axiosInstance.patch<T, R, D>(url, data, config);
  }
}

export const isApiError = isAxiosError;

export const getApiErrorMessage = (error: any) => {
  let message = "";

  if (isAxiosError(error)) {
    if (error.response) {
      message = error.response.data?.title;
    } else if (isNetworkError(error)) {
      message = "Check your internet connection.";
    } else if (isIdempotentRequestError(error)) {
      message = "Something went wrong, Please try again.";
    }
  }

  message = message ? message : "Something went wrong.\nPlease try again later.";
  return message;
};
