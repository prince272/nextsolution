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
import { createState, State } from "state-pool";

import { ExternalWindow } from "./external-window";
import { apiConfig } from "@/config/api";

export type ApiUser = {
  id: string;
  userName: string;
  firstName: string;
  lastName: string;
  email?: string;
  emailConfirmed: boolean;
  phoneNumber?: string;
  phoneNumberConfirmed: boolean;
  active: boolean;
  activeAt: Date;

  
  tokenType: string;
  accessToken: string;
  refreshToken: string;
  [key: string]: any;
};

export interface ApiState {
  user?: ApiUser | null | undefined;
}

export interface ApiConfig extends CreateAxiosDefaults {
  initialState?: ApiState;
}

export class Api {
  public config: ApiConfig;
  private axiosInstance: AxiosInstance;
  public store: State<ApiState>;

  private refreshing: boolean;
  private retryRequests: Array<() => void>;

  constructor(config?: ApiConfig) {
    const defaultConfig = {
      paramsSerializer: {
        encode: (params) => QueryString.stringify(params, { arrayFormat: "bracket" })
      },
      withCredentials: true
    } as ApiConfig;

    const { initialState, ...axiosConfig } = (config = { ...defaultConfig, ...config });

    this.axiosInstance = axios.create(axiosConfig);
    this.store = createState<ApiState>({ ...initialState } as ApiState);

    this.refreshing = false;
    this.retryRequests = [];

    // Add request interceptor
    this.axiosInstance.interceptors.request.use(
      (requestConfig) => {
        const apiState = this.store.getValue<ApiState>();

        if (apiState.user) {
          requestConfig.headers.setAuthorization(`${apiState.user.tokenType} ${apiState.user.accessToken}`);
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

          const apiState = this.store.getValue<ApiState>();
          if (!apiState.user) {
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
          return this.refresh(apiState.user.refreshToken)
            .then(({ data: user }) => {
              this.store.updateValue((apiState) => (apiState.user = user));
              this.refreshing = false;
              this.retryRequests.forEach((prom) => prom());
              this.retryRequests = [];
              return this.axiosInstance.request(originalRequest);
            })
            .catch(() => {
              this.store.updateValue((apiState) => (apiState.user = null));
              this.refreshing = false;
              this.retryRequests.forEach((prom) => prom());
              this.retryRequests = [];
              throw error;
            });
        }

        return Promise.reject(error);
      }
    );

    this.config = config;
  }

  public async signUp<T extends ApiUser, R extends AxiosResponse<T>, D extends any>(
    data: { firstName: string; lastName: string; username: string; password: string; [key: string]: any },
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/accounts/register`,
      method: "POST",
      data,
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.store.updateValue((apiState) => (apiState.user = response.data));
    return response;
  }

  public async signIn<T extends ApiUser, R extends AxiosResponse<T>, D extends any>(
    data: { username: string; password: string; [key: string]: any },
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/accounts/authenticate`,
      method: "POST",
      data,
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.store.updateValue((apiState) => (apiState.user = response.data));
    return response;
  }

  public async signInWith<T extends ApiUser, R extends AxiosResponse<T>, D extends any>(
    provider: string,
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/accounts/${provider}/authenticate`,
      method: "POST",
      ...config
    } as AxiosRequestConfig<D>;

    try {
      const externalUrl = new URL(`${this.axiosInstance.defaults.baseURL}/accounts/${provider}/authenticate`);
      externalUrl.searchParams.set("returnUrl", window.location.href);
      await ExternalWindow.open(externalUrl, { center: true });
    } catch (error) {
      console.warn(error);
    }
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.store.updateValue((apiState) => (apiState.user = response.data));
    return response;
  }

  public async refresh<T extends ApiUser, R extends AxiosResponse<T>, D extends any>(
    refreshToken: string,
    config?: AxiosRequestConfig<D>
  ): Promise<R> {
    config = {
      url: `/accounts/session/refresh`,
      method: "POST",
      data: { refreshToken },
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.store.updateValue((apiState) => (apiState.user = response.data));
    return response;
  }

  public async signOut<T extends any, R extends AxiosResponse<T>, D extends any>(config?: AxiosRequestConfig<D>): Promise<R> {
    config = {
      url: `/accounts/session/revoke`,
      method: "POST",
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.store.updateValue((apiState) => (apiState.user = null));
    return response;
  }

  public async resetPasswordCode<T extends ApiUser, R extends AxiosResponse<T>, D extends any>(
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
    return response;
  }

  public async resetPassword<T extends ApiUser, R extends AxiosResponse<T>, D extends any>(
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
    this.store.updateValue((apiState) => (apiState.user = response.data));
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

const api = new Api(apiConfig);
export const getApi = () => api;