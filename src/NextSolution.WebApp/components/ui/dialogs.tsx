import React, { ComponentType, Fragment, PropsWithChildren, useContext, useEffect, useRef } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import queryString, { ParsedUrl } from "query-string";

import { useStateAsync } from "@/lib/hooks";

export interface DialogContextProps {
  open: (id: string, props?: any) => Promise<any>;
  close: (id: string) => Promise<any>;
}

export interface DialogProps {
  id: string;
  Component: ComponentType;
  props: any;
  opened: boolean;
  mounted: boolean;
  onClose: () => void;
}

export const DialogContext = React.createContext<DialogContextProps>(undefined!);

export interface DialogContextType {
  dialogs: { id: string; Component: ComponentType<any> }[];
}

export const useDialog = () => {
  const context = useContext(DialogContext);
  if (context === undefined) {
    throw new Error("useDialog must be used within DialogProvider");
  }
  return context;
};

export const DialogProvider: React.FC<PropsWithChildren<DialogContextType>> = ({ children, dialogs: _dialogs }) => {
  const [dialogs, setDialogs] = useStateAsync<DialogProps[]>(_dialogs as any);

  const updateDialog = (id: string, dialog: Partial<DialogProps>) => {
    return setDialogs((prevDialogs) => prevDialogs.map((d) => (d.id === id ? { ...d, ...dialog } : d)));
  };

  const context = useRef<DialogContextProps>({
    open: async (id, props) => {
      await updateDialog(id, {
        ...props,
        opened: true,
        mounted: true
      });
    },
    close: async (id) => {
      await updateDialog(id, { opened: false, mounted: true });
      await new Promise((resolve) => setTimeout(resolve, 150));
      await updateDialog(id, { opened: false, mounted: false });
    }
  });

  return (
    <DialogContext.Provider value={context.current}>
      {children}
      {dialogs.map(({ Component, id, mounted, props, ...dialogProps }) => {
        return mounted ? <Component key={id} {...dialogProps} {...props} /> : <Fragment key={id}></Fragment>;
      })}
    </DialogContext.Provider>
  );
};

export const DIALOG_QUERY_NAME = "dialogId";

export const DialogRouter: React.FC<{ loaded: boolean }> = ({ loaded }) => {
  const dialog = useContext(DialogContext);

  if (dialog === undefined) {
    throw new Error("DialogRouter must be used within DialogProvider");
  }

  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const routesRef = useRef<{ pathname: string; searchParams: URLSearchParams }[]>([]);

  useEffect(() => {
    routesRef.current.push({ pathname, searchParams: new URLSearchParams(Array.from(searchParams.entries())) });
    if (routesRef.current.length == 1 && routesRef.current[0].searchParams.has(DIALOG_QUERY_NAME))
      routesRef.current.unshift({ pathname: "/", searchParams: new URLSearchParams() });
  }, [pathname, searchParams]);

  useEffect(() => {
    (async () => {
      if (!loaded) return;

      const currentRoute = routesRef.current[routesRef.current.length - 1];
      const dialogId = currentRoute?.searchParams.get(DIALOG_QUERY_NAME);

      const returnRoute =
        routesRef.current
          .slice()
          .reverse()
          .filter((route) => route.pathname != currentRoute.pathname)
          .find((route, index) => route.searchParams.has("return") || index === 1) ?? routesRef.current[0];

      const returnHref = queryString.stringifyUrl({ url: returnRoute.pathname, query: Object.fromEntries(returnRoute.searchParams) });

      if (dialogId) {
        const prevRoute = routesRef.current[routesRef.current.length - 2];
        const prevDialogId = prevRoute?.searchParams.get(DIALOG_QUERY_NAME);
        if (prevDialogId) await dialog.close(prevDialogId);

        await dialog.open(dialogId, {
          onClose: () => {
            router.push(returnHref);
            dialog.close(dialogId);
          }
        });
      }
    })();
  }, [loaded, dialog, router, pathname, searchParams]);

  return <></>;
};
