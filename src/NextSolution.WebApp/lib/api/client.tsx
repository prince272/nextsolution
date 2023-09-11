"use client";

import { createContext, FC, ReactNode, useContext, useRef } from "react";

import { useCookies } from "../cookies/client";
import { Api } from "./core";
import { ApiConfig, User } from "./types";

const ApiContext = createContext<Api>(undefined!);

export const useApi = (): Api => {
  const context = useContext(ApiContext);
  if (context === undefined) {
    throw new Error("useApi must be used within ApiProvider");
  }
  return context;
};

export const useUser = (): User | null | undefined => {
  const context = useContext(ApiContext);
  if (context === undefined) {
    throw new Error("useUser must be used within ApiProvider");
  }

  const [{ user }] = context.state.useState();
  return user;
};

export const ApiProvider: FC<{ config: ApiConfig; children: ReactNode }> = ({ config, children }) => {
  const cookies = useCookies();
  const api = useRef(new Api({ ...config, store: cookies })).current;
  return <ApiContext.Provider value={api}>{children}</ApiContext.Provider>;
};
