"use client";

import { createContext, FC, ReactNode, useContext, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { DialogProvider, DialogRouter } from "@/dialogs/provider";
import { NextUIProvider } from "@nextui-org/system";
import { ThemeProvider as NextThemesProvider } from "next-themes";
import queryString from "query-string";
import { Toaster, ToastOptions } from "react-hot-toast";

import { useApi, useUser } from "@/lib/api/client";
import { ExternalWindow } from "@/lib/external-window";
import { useDebounceCallback } from "@/lib/hooks/useDebounceCallback";
import { SignalRProvider } from "@/lib/signalr";

export interface AppContextType {
  authenticate: () => void;
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
  const router = useRouter();
  const currentUser = useUser();
  const [loading, setLoading] = useState(true);
  const [sidebarOpened, setSidebarOpened] = useState(false);
  const [mounted, setMounted] = useState(false);
  const authenticateCallback = useDebounceCallback(500);

  useEffect(() => {
    setMounted(true);
    setTimeout(() => {
      setLoading(false);
      ExternalWindow.notify();
    }, 1500);
  }, []);

  const value = {
    authenticate: () =>
      authenticateCallback(() => {
        if (!currentUser) router.replace(queryString.stringifyUrl({ url: "/", query: { dialogId: "sign-in" } }));
      }),
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
