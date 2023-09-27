"use client";

import { ReactNode, useEffect, useState } from "react";
import { DialogProvider, DialogRouter } from "@/ui/dialog-provider";
import { NextUIProvider } from "@nextui-org/system";
import { ThemeProvider as NextThemesProvider } from "next-themes";
import { Toaster, ToastOptions } from "react-hot-toast";
import * as zustand from "zustand";

import { useApi, useUser } from "@/lib/api/client";
import { ExternalWindow } from "@/lib/external-window";
import { SignalRProvider } from "@/lib/signalr";

import * as dialogs from "./dialogs";

export interface AppProviderProps {
  children: ReactNode;
}

export function AppProvider({ children }: AppProviderProps) {
  const api = useApi();
  const { finishLoading } = useAppStore();
  const currentUser = useUser();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
    setTimeout(() => {
      finishLoading();
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
            <DialogRouter />
            {children}
          </DialogProvider>
        </NextThemesProvider>
        <Toaster toastOptions={toastOptions} />
      </NextUIProvider>
    </SignalRProvider>
  );
}

export type AppState = {
  loading: boolean;
};

export type AppActions = {
  startLoading: () => void;
  finishLoading: () => void;
};

export const useAppStore = zustand.create<AppState & AppActions>((set) => ({
  loading: true,
  startLoading: () => set((state) => ({ ...state, loading: true })),
  finishLoading: () => set((state) => ({ ...state, loading: false }))
}));
