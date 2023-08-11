import React, { ComponentType, PropsWithChildren, useContext, useEffect, useRef, useState } from "react";
import { ReadonlyURLSearchParams, usePathname, useRouter, useSearchParams } from "next/navigation";
import { usePrevious, useStateAsync } from "@/utils/hooks";
import queryString from "query-string";

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
  onOpen: () => void;
  onClose: () => void;
}

export const DialogContext = React.createContext<DialogContextProps>(null!);

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
    open: async (id, { onOpen, onClose, ...props }) => {
      await updateDialog(id, {
        ...props,
        onOpen: () => {
          onOpen && onOpen();
          context.current.open(id);
        },
        onClose: () => {
          onClose && onClose();
          context.current.close(id);
        },
        opened: false,
        mounted: true
      });
      await updateDialog(id, { opened: true, mounted: true });
      await new Promise((resolve) => setTimeout(resolve, 300));
    },
    close: async (id) => {
      await updateDialog(id, { opened: false, mounted: true });
      await new Promise((resolve) => setTimeout(resolve, 300));
      await updateDialog(id, { opened: false, mounted: false });
    }
  });

  return (
    <DialogContext.Provider value={context.current}>
      {children}
      {dialogs
        .filter((d) => d.mounted)
        .map(({ Component, id, props, ...dialogProps }) => {
          return <Component key={id} {...dialogProps} {...props} />;
        })}
      <DialogRouter />
    </DialogContext.Provider>
  );
};

export const DIALOG_QUERY_NAME = "dialogId";

export const DialogRouter: React.FC = () => {
  const dialog = useContext(DialogContext);

  if (dialog === undefined) {
    throw new Error("DialogRouter must be used within DialogProvider");
  }

  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const prevSearchParams = usePrevious(searchParams);
  const tempSearchParams = usePrevious(searchParams, (prev, next) => prev.has(DIALOG_QUERY_NAME))

  useEffect(() => {
    (async () => {
      const prevDialogId = prevSearchParams?.get(DIALOG_QUERY_NAME);
      if (prevDialogId) await dialog.close(prevDialogId);

      const dialogId = searchParams.get(DIALOG_QUERY_NAME);

      if (dialogId) {
        const prevHref = queryString.stringifyUrl({
          url: pathname,
          query: tempSearchParams ? Object.fromEntries(tempSearchParams) : {}
        });

        await dialog.open(dialogId, {
          ...Object.fromEntries(searchParams),
          onClose: () => router.replace(prevHref)
        });
      }
    })();
  }, [dialog, router, pathname, searchParams, prevSearchParams, tempSearchParams]);

  return <></>;
};
