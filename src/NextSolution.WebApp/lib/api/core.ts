import axios, { AxiosError, AxiosInstance, AxiosRequestConfig, AxiosResponse, CreateAxiosDefaults, HttpStatusCode, isAxiosError } from "axios";
import QueryString from "query-string";
import { createState, State } from "state-pool";

import { ExternalWindow } from "../external-window";
import { ApiConfig, ApiState, ApiStore, User } from "./types";
import { parseJSON, stringifyJSON } from "../utils";

const INITIAL_API_STATE = "__INITIAL_API_STATE__";

export class Api {
  private axiosInstance: AxiosInstance;
  public config: ApiConfig;
  private store: ApiStore;
  public state: State<ApiState>;

  private refreshing: boolean;
  private retryRequests: Array<() => void>;

  constructor(config: ApiConfig) {
    const defaultAxiosConfig = {
      paramsSerializer: {
        encode: (params) => QueryString.stringify(params, { arrayFormat: "bracket" })
      },
      withCredentials: true
    } as CreateAxiosDefaults;

    const { store, ...axiosConfig } = (config = { ...defaultAxiosConfig, ...config });

    this.axiosInstance = axios.create(axiosConfig);
    this.config = config;
    this.store = store;
    this.state = createState(parseJSON(store.get(INITIAL_API_STATE)) ?? {});
    this.state.subscribe<ApiState>((state) => {
      this.store.set(INITIAL_API_STATE, stringifyJSON(state));
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

          const currentState = this.state.getValue<ApiState>();
          if (!currentState.user) {
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
          return this.refresh(currentState.user.refreshToken)
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
      url: `/accounts/register`,
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
      url: `/accounts/authenticate`,
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
    this.state.updateValue((currentState) => (currentState.user = response.data));
    return response;
  }

  public async refresh<T extends User, R extends AxiosResponse<T>, D extends any>(refreshToken: string, config?: AxiosRequestConfig<D>): Promise<R> {
    config = {
      url: `/accounts/session/refresh`,
      method: "POST",
      data: { refreshToken },
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.state.updateValue((currentState) => (currentState.user = response.data));
    return response;
  }

  public async signOut<T extends any, R extends AxiosResponse<T>, D extends any>(config?: AxiosRequestConfig<D>): Promise<R> {
    config = {
      url: `/accounts/session/revoke`,
      method: "POST",
      ...config
    } as AxiosRequestConfig<D>;
    const response = await this.axiosInstance.request<T, R, D>(config);
    this.state.updateValue((currentState) => (currentState.user = null));
    return response;
  }

  public async resetPasswordCode<T extends User, R extends AxiosResponse<T>, D extends any>(
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

  public async resetPassword<T extends User, R extends AxiosResponse<T>, D extends any>(
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