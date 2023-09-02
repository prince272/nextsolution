"use client";

import React, { PropsWithChildren, useCallback, useContext, useEffect, useRef, useState } from "react";
import { NextUIProvider, Spinner } from "@nextui-org/react";
import { ThemeProvider as NextThemesProvider } from "next-themes";
import { Toaster } from "react-hot-toast";
import {
  createSignalRContext // SignalR
} from "react-signalr";

import { apiConfig } from "@/config/api";
import { Api, ApiConfig, ApiState, ApiUser, getApi } from "@/lib/api";
import { ExternalWindow } from "@/lib/external-window";
import { DialogProvider, DialogRouter } from "@/components/ui/dialogs";

import { dialogs } from "./dialogs";

const { useSignalREffect, Provider: SignalRProvider, ...signalR } = createSignalRContext();

const useSignalR = () => signalR;
export { useSignalR, useSignalREffect };

export function App({ children }: { children: React.ReactNode }) {
  const [loaded, setLoaded] = useState(false);
  const api = getApi();
  const [{ user: currentUser }] = api.store.useState();

  useEffect(() => {
    setTimeout(() => {
      setLoaded(true);
      ExternalWindow.notify();
    }, 1500);
  }, []);

  return (
    <SignalRProvider
      withCredentials={api.config.withCredentials}
      automaticReconnect={true}
      connectEnabled={true}
      accessTokenFactory={() => currentUser?.accessToken!}
      dependencies={[currentUser]} // remove previous connection and create a new connection if changed
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
}
