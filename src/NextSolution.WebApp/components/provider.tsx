"use client";

import { createContext, FC, ReactNode, useContext, useEffect, useState } from "react";
import { DialogProvider, DialogRouter } from "@/dialogs/provider";
import { NextUIProvider } from "@nextui-org/system";
import { ThemeProvider as NextThemesProvider } from "next-themes";
import { Toaster, ToastOptions } from "react-hot-toast";

import { useApi, useUser } from "@/lib/api/client";
import { ExternalWindow } from "@/lib/external-window";
import { SignalRProvider } from "@/lib/signalr";

export interface AppContextType {
  loading: boolean;
  sidebar: {
    open: () => void;
    close: () => void;
    toggle: () => void;
    opened: boolean;
  };
}

const AppContext = createContext<AppContextType>(undefined!);

export const useApp = () => {
  const context = useContext(AppContext);
  if (context === undefined) {
    throw new Error("useApp must be used within AppProvider");
  }
  return context;
};

export interface AppProviderProps {
  children: ReactNode;
}

export function AppProvider({ children }: AppProviderProps) {
  const api = useApi();
  const currentUser = useUser();
  const [loading, setLoading] = useState(true);
  const [sidebarOpened, setSidebarOpened] = useState(false);
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
    setTimeout(() => {
      setLoading(false);
      ExternalWindow.notify();
    }, 1500);
  }, []);

  const value = {
    loading,
    sidebar: {
      opened: sidebarOpened,
      open: () => setSidebarOpened(true),
      close: () => setSidebarOpened(false),
      toggle: () => setSidebarOpened((prev) => !prev)
    }
  };

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
    <AppContext.Provider value={value}>
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
            <DialogProvider>
              <DialogRouter loading={loading} />
              {children}
            </DialogProvider>
          </NextThemesProvider>
          <Toaster toastOptions={toastOptions} />
        </NextUIProvider>
      </SignalRProvider>
    </AppContext.Provider>
  );
}
