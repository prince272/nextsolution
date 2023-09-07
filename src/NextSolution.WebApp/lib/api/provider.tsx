"use client";

import { createContext, FC, ReactNode, useCallback, useContext, useEffect, useMemo, useRef } from "react";

import { useCookies } from "../cookies/provider";
import { Api } from "./core";
import { ApiConfig, User } from "./types";

export interface useUserProps {
  onUnauthenticated?: () => void;
}

const ApiContext = createContext<Api>(undefined!);

export const useApi = (): Api => {
  const context = useContext(ApiContext);
  if (context === undefined) {
    throw new Error("useApi must be used within ApiProvider");
  }
  return context;
};

export const useUser = (props?: useUserProps): User | null | undefined => {
  const context = useContext(ApiContext);
  if (context === undefined) {
    throw new Error("useUser must be used within ApiProvider");
  }

  const [{ user }] = context.state.useState();
  const onUnauthenticated = useCallback(() => {
    props?.onUnauthenticated?.();
  }, [props]);

  useMemo(() => {
    if (!user) {
      onUnauthenticated?.();
    }
  }, [onUnauthenticated, user]);

  return user;
};

export const ApiProvider: FC<{ config: ApiConfig; children: ReactNode }> = ({ config, children }) => {
  const cookies = useCookies();
  const api = useRef(new Api({ ...config, store: cookies })).current;
  return <ApiContext.Provider value={api}>{children}</ApiContext.Provider>;
};
