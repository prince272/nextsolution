"use client";

import { Component, createContext, FC, ReactNode, useContext, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { DialogProvider, DialogRouter } from "@/ui/dialog-provider";
import { NextUIProvider } from "@nextui-org/system";
import { ThemeProvider as NextThemesProvider } from "next-themes";
import { Toaster, ToastOptions } from "react-hot-toast";

import { useApi, useUser } from "@/lib/api/client";
import { ExternalWindow } from "@/lib/external-window";
import { useLocalStorage } from "@/lib/hooks";
import { SignalRProvider } from "@/lib/signalr";

import * as dialogs from "./dialogs";
import { useAppStore } from "./state";

export interface AppProviderProps {
  children: ReactNode;
}

export function AppProvider({ children }: AppProviderProps) {
  const api = useApi();
  const app = useAppStore();
  const currentUser = useUser();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
    setTimeout(() => {
      app.completeLoading();
      ExternalWindow.notify();
    }, 1500);
  }, []);

  const toastOptions = {
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
  } as ToastOptions;

  return (
    <SignalRProvider
      withCredentials={api.config.withCredentials}
      automaticReconnect={true}
      connectEnabled={mounted}
      accessTokenFactory={currentUser ? () => currentUser.accessToken : undefined}
      dependencies={[currentUser]} // remove previous connection and create a new connection if changed
      url={new URL("/signalr", api.config.baseURL).toString()}
    >
      <NextUIProvider>
        <NextThemesProvider {...{ attribute: "class", defaultTheme: "dark" }}>
          <DialogProvider components={Object.entries(dialogs).map(([name, Component]) => ({ name, Component }))}>
            <DialogRouter loading={app.loading} />
            {children}
          </DialogProvider>
        </NextThemesProvider>
        <Toaster toastOptions={toastOptions} />
      </NextUIProvider>
    </SignalRProvider>
  );
}
