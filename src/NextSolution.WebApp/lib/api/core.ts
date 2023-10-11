import axios, { AxiosError, AxiosInstance, AxiosRequestConfig, AxiosResponse, HttpStatusCode, isAxiosError } from "axios";
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
    const defaultAxiosConfig = {} as ApiConfig;

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
    const response = await this.request<T, R, D>(config);
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
    const response = await this.request<T, R, D>(config);
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
      const externalUrl = new URL(`${this.config.baseURL}/users/${provider}/session/generate`);
      externalUrl.searchParams.set("returnUrl", window.location.href);
      await ExternalWindow.open(externalUrl, { center: true });
    } catch (error) {
      console.warn(error);
    }
    const response = await this.request<T, R, D>(config);
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
      const response = await this.request<T, R, D>(config);
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
    return this.request<T, R, D>(config).finally(() => {
      this.user.next(null);
    });
  }

  private handleError(error: any, callback: (config: AxiosRequestConfig<any>) => any) {
    if (isAxiosError(error) && error.response) {
      const { config, response } = error;
      const originalRequest = config as AxiosRequestConfig & { retryCount: number };

      if (response.status != HttpStatusCode.Unauthorized) {
        throw error;
      }

      originalRequest.retryCount = (originalRequest.retryCount ?? 0) + 1;
      if (originalRequest.retryCount > 2) {
        // If already retried twice, reject the request
        throw error;
      }

      const currentUser = this.user.getValue();
      if (!currentUser) {
        throw error;
      }

      if (this.refreshing) {
        // If already refreshing, add the failed request to the queue
        const retryOriginalRequest = new Promise<any>((resolve) => {
          this.retryRequests.push(() => resolve(callback(originalRequest!)));
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
          const retryOriginalRequest = callback(originalRequest);
          return retryOriginalRequest;
        })
        .catch((innerError) => {
          this.user.next(null);
          this.refreshing = false;
          this.retryRequests.forEach((prom) => prom());
          this.retryRequests = [];
          throw error;
        });
    }

    throw error;
  }

  public request<T = any, R = AxiosResponse<T>, D = any>(config: AxiosRequestConfig<D>): Promise<R> {
    const currentUser = this.user.getValue();

    if (currentUser) {
      config = {
        ...config,
        headers: {
          ...config?.headers,
          Authorization: `${currentUser.tokenType} ${currentUser.accessToken}`
        }
      };
    } else {
      delete config?.headers?.Authorization;
    }

    return this.axiosInstance
      .request<T, R, D>(config)
      .then((response) => response)
      .catch((error) => this.handleError(error, this.request.bind(this)));
  }

  public get<T = any, R = AxiosResponse<T>, D = any>(url: string, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.request<T, R, D>({ ...config, url, method: "GET" });
  }

  public delete<T = any, R = AxiosResponse<T>, D = any>(url: string, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.request<T, R, D>({ ...config, url, method: "DELETE" });
  }

  public head<T = any, R = AxiosResponse<T>, D = any>(url: string, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.request<T, R, D>({ ...config, url, method: "HEAD" });
  }

  public options<T = any, R = AxiosResponse<T>, D = any>(url: string, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.request<T, R, D>({ ...config, url, method: "OPTIONS" });
  }

  public post<T = any, R = AxiosResponse<T>, D = any>(url: string, data?: D, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.request<T, R, D>({ ...config, url, method: "POST", data });
  }

  public put<T = any, R = AxiosResponse<T>, D = any>(url: string, data?: D, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.request<T, R, D>({ ...config, url, method: "PUT", data });
  }

  public patch<T = any, R = AxiosResponse<T>, D = any>(url: string, data?: D, config?: AxiosRequestConfig<D>): Promise<R> {
    return this.request<T, R, D>({ ...config, url, method: "PATCH", data });
  }

  public async stream<D = any>(config: AxiosRequestConfig<D>): Promise<Response> {
    config = { ...this.config, ...config };

    const currentUser = this.user.getValue();

    if (currentUser) {
      config = {
        ...config,
        headers: {
          ...config?.headers,
          Authorization: `${currentUser.tokenType} ${currentUser.accessToken}`
        }
      };
    } else {
      delete config?.headers?.Authorization;
    }

    const fetchInit: RequestInit = {};

    if (config.method) {
      fetchInit.method = config.method;
    }

    if (config.headers) {
      const fetchHeaders = new Headers();
      for (const key of Object.keys(config.headers)) {
        fetchHeaders.append(key, config.headers[key]);
      }
      fetchInit.headers = fetchHeaders;
    }

    if (config.data) {
      fetchInit.body = JSON.stringify(config.data);

      fetchInit.headers = fetchInit.headers || new Headers();
      if (fetchInit.headers instanceof Headers) {
        fetchInit.headers.set("Accept", "application/json");
        fetchInit.headers.set("Content-Type", "application/json");
      }
    }

    if (config.params) {
      const queryString = new URLSearchParams(config.params).toString();
      config.url += `?${queryString}`;
    }

    if (config.auth) {
      const basicAuthHeader = `Basic ${btoa(`${config.auth.username}:${config.auth.password}`)}`;
      fetchInit.headers = fetchInit.headers || new Headers();

      if (fetchInit.headers instanceof Headers) {
        fetchInit.headers.set("Authorization", basicAuthHeader);
      }
    }

    if (config.timeout) {
      const abortController = new AbortController();
      fetchInit.signal = abortController.signal;

      setTimeout(() => {
        // Abort the fetch request when the timeout is reached
        abortController.abort();
      }, config.timeout);
    }

    fetchInit.credentials = config.withCredentials ? "include" : fetchInit.credentials;

    return fetch(`${config.baseURL}${config.url}`, fetchInit)
      .then(async (response) => {
        if (!response.ok) {
          const data = await response.json().catch(() => response.text());

          const error = new AxiosError(
            "Request failed with status code " + response.status,
            [AxiosError.ERR_BAD_REQUEST, AxiosError.ERR_BAD_RESPONSE][Math.floor(response.status / 100) - 4],
            config as any,
            fetchInit,
            {
              data,
              status: response.status,
              statusText: response.statusText
            } as AxiosResponse
          );

          throw error;
        } else {
          // API Success
          return response;
        }
      })
      .catch((error) => this.handleError(error, this.stream.bind(this)));
  }
}
