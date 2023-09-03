import { getCookies } from "../cookies/server";
import { apiConfig } from "./config";
import { Api } from "./core";
import { ApiState, User } from "./types";

let api: Api | null = null;

export const getApi = (): Api => {
  if (!api) {
    const cookies = getCookies();
    api = new Api({ ...apiConfig, store: cookies });
  }
  return api;
};

export const getUser = (): User | null | undefined => {
  if (!api) {
    const cookies = getCookies();
    api = new Api({ ...apiConfig, store: cookies });
  }
  return api.state.getValue<ApiState>().user;
};
