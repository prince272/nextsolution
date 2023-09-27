"use client";

import { createContext, FC, ReactNode, useContext, useEffect, useRef, useState } from "react";

import { useCookies } from "../cookies/client";
import { useDebounceCallback } from "../hooks";
import { Api } from "./core";
import { ApiConfig, User } from "./types";

const ApiContext = createContext<Api>(undefined!);

export const useApi = (): Api => {
  const api = useContext(ApiContext);
  if (api === undefined) {
    throw new Error("useApi must be used within ApiProvider");
  }
  return api;
};

export const useUser = (): User | null | undefined => {
  const api = useContext(ApiContext);
  if (api === undefined) {
    throw new Error("useUser must be used within ApiProvider");
  }

  const [user, setUser] = useState<User | null | undefined>(api.user.value);

  useEffect(() => {
    const subscription = api.user.subscribe((currentUser) => {
      setUser(currentUser);
    });

    return () => {
      subscription.unsubscribe();
    };
  }, []);

  return user;
};

export const useUnauthenticated = (fn: () => void) => {
  const currentUser = useUser();
  const authenticateCallback = useDebounceCallback(1000);
  authenticateCallback(() => {
    if (!currentUser) fn();
  });
};

export const ApiProvider: FC<{ config: ApiConfig; children: ReactNode }> = ({ config, children }) => {
  const cookies = useCookies();
  const api = useRef(new Api({ ...config, store: cookies })).current;
  return <ApiContext.Provider value={api}>{children}</ApiContext.Provider>;
};
