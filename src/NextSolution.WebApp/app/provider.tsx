// app/providers.tsx
"use client";

import React, { PropsWithChildren, useContext, useEffect, useRef, useState } from "react";
import { NextUIProvider } from "@nextui-org/react";
import { ThemeProvider as NextThemesProvider } from "next-themes";
import * as rx from "rxjs";

import { Api, ApiConfig, ApiTokens } from "@/lib/api";

export const ApiContext = React.createContext<Api>(null!);

export const useApi = () => {
  const context = useContext(ApiContext);
  if (context === undefined) {
    throw new Error("useApi must be used within ApiProvider");
  }
  return context;
};

export const ApiProvider: React.FC<PropsWithChildren<{ config: ApiConfig }>> = ({ children, config }) => {
  const apiRef = useRef(new Api(config));

  return <ApiContext.Provider value={apiRef.current}>{children}</ApiContext.Provider>;
};

export function AppProvider({ children }: { children: React.ReactNode }) {
  return (
    <NextUIProvider>
      <NextThemesProvider attribute="class" defaultTheme="dark">
        <UserProvider>{children}</UserProvider>
      </NextThemesProvider>
    </NextUIProvider>
  );
}

export type User = {
  id: string,
  userName: string
  firstName: string,
  lastName: string,
  email?: string,
  emailConfirmed: boolean,
  phoneNumber?: string,
  phoneNumberConfirmed: boolean,
  active: boolean,
  activeAt: Date,
    // Add additional properties here.
} & ApiTokens;

export const UserContext = React.createContext<User | null | undefined>(null);

export const useUser = () => {
  const context = useContext(UserContext);
  return context;
};

export const UserProvider: React.FC<PropsWithChildren> = ({ children }) => {
  const api = useApi();
  const [value, setValue] = useState<User | null | undefined>(api.tokenStore.getValue() as User);

  useEffect(() => {
    const subscription = api.tokenStore.pipe(rx.skip(1)).subscribe((value) => setValue(value as User));
    return () => subscription.unsubscribe();
  }, [api]);

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>;
};
