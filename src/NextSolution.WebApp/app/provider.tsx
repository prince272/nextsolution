"use client";

import { ComponentType, createContext, FC, Fragment, ReactNode, useEffect, useRef, useState } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { NextUIProvider } from "@nextui-org/system";
import { ThemeProvider as NextThemesProvider } from "next-themes";
import { ThemeProviderProps } from "next-themes/dist/types";
import queryString from "query-string";
import { Toaster, ToastOptions } from "react-hot-toast";

import { useApi, useUser } from "@/lib/api/provider";
import { ExternalWindow } from "@/lib/external-window";
import { usePrevious } from "@/lib/hooks";
import { isEqualSearchParams } from "@/lib/utils";
import { DialogProvider, useDialog } from "@/components/ui/dialog-provider";
import { AppLayout } from "@/components/app/layout";
import { AppLoader } from "@/components/app/loader";
import { dialogs } from "@/components/dialogs";
import { polly } from "@/components/polly";
import { SignalRProvider } from "@/components/signalr";
import { SiteLayout } from "@/components/site/layout";

export interface AppContextType {
  loading: boolean;
  navigation: {
    open: () => void;
    close: () => void;
    opened: boolean;
  };
}

const AppContext = createContext<AppContextType>(undefined!);

export interface AppProviderProps {
  children: ReactNode;
  themeProps?: ThemeProviderProps;
}

export function AppProvider({ children, themeProps }: AppProviderProps) {
  const api = useApi();
  const currentUser = useUser();
  const [loading, setLoading] = useState(true);
  const [navigationOpened, setNavigationOpened] = useState(false);

  useEffect(() => {
    setTimeout(() => {
      setLoading(false);
      ExternalWindow.notify();
    }, 1500);
  }, []);

  const value = {
    loading,
    navigation: {
      opened: navigationOpened,
      open: () => setNavigationOpened(true),
      close: () => setNavigationOpened(false)
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
        connectEnabled={true}
        accessTokenFactory={currentUser ? () => currentUser.accessToken : undefined}
        dependencies={[currentUser]} // remove previous connection and create a new connection if changed
        logMessageContent={false}
        url={new URL("/signalr", api.config.baseURL).toString()}
      >
        <NextUIProvider>
          <NextThemesProvider {...themeProps}>
            <DialogProvider dialogs={dialogs}>
              <AppLoader loading={loading}>
                <Layout as={currentUser ? AppLayout : SiteLayout}>{children}</Layout>
              </AppLoader>
              <AppDialogRouter loading={loading} />
            </DialogProvider>
          </NextThemesProvider>
          <Toaster toastOptions={toastOptions} />
        </NextUIProvider>
      </SignalRProvider>
    </AppContext.Provider>
  );
}

export const DIALOG_QUERY_NAME = "dialogId";

export const SIGN_IN_DIALOG_ID = "sign-in";

const AppDialogRouter: FC<{ loading: boolean }> = ({ loading }) => {
  const dialog = useDialog();

  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const prevPathname = usePrevious(pathname);
  const prevSearchParams = usePrevious(searchParams, (prev, current) => isEqualSearchParams(prev, current));

  const returnHref = useRef("/");

  useEffect(() => {
    (async () => {
      if (loading) return;

      const dialogId = searchParams.get(DIALOG_QUERY_NAME);

      if (dialogId) {
        if (prevPathname && prevSearchParams) {
          const prevDialogId = prevSearchParams.get(DIALOG_QUERY_NAME);
          returnHref.current = prevDialogId ? returnHref.current : queryString.stringifyUrl({ url: prevPathname, query: Object.fromEntries(prevSearchParams) });
        }

        const prevDialogId = prevSearchParams?.get(DIALOG_QUERY_NAME);
        if (prevDialogId) await dialog.close(prevDialogId);

        await dialog.open(dialogId, {
          onClose: async () => {
            // Close the dialog when it's closed.
            await dialog.close(dialogId);

            // If the closed dialog was the sign-in dialog, reset returnHref to "/".
            if (dialogId == SIGN_IN_DIALOG_ID) {
              returnHref.current = "/";
            }

            // Push the new route using router with the updated returnHref.
            router.push(returnHref.current);
          }
        });
      } else {
        returnHref.current = "/";
      }
    })();
  }, [dialog, loading, pathname, prevPathname, prevSearchParams, router, searchParams]);

  return <></>;
};

interface LayoutProps {
  children: ReactNode;
}

const Layout = polly<ComponentType, LayoutProps>(function Component({ as: Component = Fragment, children, ...rest }, ref?) {
  return (
    <Component {...rest} ref={ref}>
      {children}
    </Component>
  );
});
