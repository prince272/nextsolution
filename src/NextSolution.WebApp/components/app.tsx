"use client";

import { FC, ReactNode, useEffect, useState } from "react";
import { NextUIProvider, Spinner } from "@nextui-org/react";
import { ThemeProvider as NextThemesProvider } from "next-themes";
import { Toaster } from "react-hot-toast";

import { useApi, useUser } from "@/lib/api";
import { ExternalWindow } from "@/lib/external-window";
import { SignalRProvider as SignalRContextProvider, SignalRLogLevel } from "@/lib/signalr";

import { dialogs } from "./dialogs";
import { DialogProvider, DialogRouter } from "./ui/dialogs";

export const AppProvider: FC<{ children: ReactNode }> = ({ children }) => {
  const [loaded, setLoaded] = useState(false);
  const api = useApi();
  const user = useUser();

  useEffect(() => {
    setTimeout(() => {
      setLoaded(true);
      ExternalWindow.notify();
    }, 1500);
  }, []);

  return (
    <SignalRContextProvider
      withCredentials={api.config.withCredentials}
      automaticReconnect={true}
      connectEnabled={true}
      accessTokenFactory={() => user?.accessToken!}
      dependencies={[user]} // remove previous connection and create a new connection if changed
      logger={SignalRLogLevel.None}
      url={new URL("/chat", api.config.baseURL).toString()}
    >
      <NextThemesProvider attribute="class" defaultTheme="dark">
        <NextUIProvider>
          {!loaded && (
            <div className="absolute z-[99999] flex h-full w-full flex-col items-center justify-center bg-background">
              <Spinner id="app-progress" size="lg" aria-label="Loading..." />
            </div>
          )}
          {children}
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
        </NextUIProvider>
      </NextThemesProvider>
    </SignalRContextProvider>
  );
};
