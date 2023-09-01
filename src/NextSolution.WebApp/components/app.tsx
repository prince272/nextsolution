"use client";

import React, { PropsWithChildren, useCallback, useContext, useEffect, useRef, useState } from "react";
import { NextUIProvider, Spinner } from "@nextui-org/react";
import { ThemeProvider as NextThemesProvider } from "next-themes";
import { Toaster } from "react-hot-toast";
import {
  createSignalRContext // SignalR
} from "react-signalr";
import * as rx from "rxjs";

import { apiConfig } from "@/config/api";
import { Api, ApiConfig, ApiTokens } from "@/lib/api";
import { ExternalWindow } from "@/lib/external-window";
import { DialogProvider, DialogRouter } from "@/components/ui/dialogs";

import { dialogs } from "./dialogs";

const { useSignalREffect, Provider: SignalRProvider, ...signalR } = createSignalRContext();

const useSignalR = () => signalR;
export { useSignalR, useSignalREffect };

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
  }, []);

  return <ApiContext.Provider value={{ api: apiRef.current, user }}>{children}</ApiContext.Provider>;
};

export const ApiConsumer = ApiContext.Consumer;

export function App({ children }: { children: React.ReactNode }) {
  const [loaded, setLoaded] = useState(false);

  useEffect(() => {
    setTimeout(() => {
      setLoaded(true);
      ExternalWindow.notify();
    }, 1500);
  }, []);

  return (
    <ApiProvider config={apiConfig}>
      <ApiConsumer>
        {({ api, user }) => {
          return (
            <SignalRProvider
              withCredentials={api.config.withCredentials}
              automaticReconnect={true}
              connectEnabled={true}
              accessTokenFactory={() => user?.accessToken!}
              dependencies={[user]} // remove previous connection and create a new connection if changed
              url={new URL("/chat", api.config.baseURL).toString()}
            >
              <NextUIProvider>
                <NextThemesProvider attribute="class" defaultTheme="dark">
                  {!loaded && (
                    <div className="absolute z-[99999] flex h-full w-full flex-col items-center justify-center bg-background">
                      <Spinner id="app-progress" size="lg" aria-label="Loading..." />
                    </div>
                  )}
                  <DialogProvider dialogs={dialogs}>
                    <div className={`${!loaded ? "hidden" : ""}`}> {children}</div>
                    <DialogRouter loaded={loaded} />
                  </DialogProvider>
                  <Toaster
                    toastOptions={{
                      duration: 5000,
                      success: {
                        className: "!bg-content1 !text-current !text-sm",
                        iconTheme: {
                          primary: "hsl(var(--nextui-success-400))",
                          secondary: "white"
                        }
                      },
                      error: {
                        className: "!bg-content1 !text-current !text-sm",
                        iconTheme: {
                          primary: "hsl(var(--nextui-danger-400))",
                          secondary: "white"
                        }
                      }
                    }}
                  />
                </NextThemesProvider>
              </NextUIProvider>
            </SignalRProvider>
          );
        }}
      </ApiConsumer>
    </ApiProvider>
  );
}
