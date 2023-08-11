"use client";

import React, { PropsWithChildren, useContext, useEffect, useRef, useState } from "react";
import * as rx from "rxjs";

import { Api, ApiConfig, ApiTokens } from "@/lib/api";

export type User = {
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
  // Add additional properties here.
} & ApiTokens;

export const ApiContext = React.createContext<{ api: Api; user: User | null | undefined }>(null!);

export const useApi = () => {
  const context = useContext(ApiContext);
  if (context === undefined) {
    throw new Error("useApi must be used within ApiProvider");
  }
  return context.api;
};

export const useUser = () => {
  const context = useContext(ApiContext);
  if (context === undefined) {
    throw new Error("useUser must be used within ApiProvider");
  }
  return context.user;
};

export const ApiProvider: React.FC<PropsWithChildren<{ config: ApiConfig }>> = ({ children, config }) => {
  const apiRef = useRef(new Api(config));
  const [user, setUser] = useState<User | null | undefined>(apiRef.current.tokenStore.getValue() as User);

  useEffect(() => {
    const subscription = apiRef.current.tokenStore.pipe(rx.skip(1)).subscribe((value) => setUser(value as User));
    return () => subscription.unsubscribe();
  }, [apiRef]);

  return <ApiContext.Provider value={{ api: apiRef.current, user }}>{children}</ApiContext.Provider>;
};
