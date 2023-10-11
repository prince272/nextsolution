"use client";

import { ComponentType, createContext, FC, Fragment, ReactNode, useContext, useEffect, useRef, useState } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import PQueue from "p-queue";
import queryString from "query-string";

import { usePreviousValue } from "@/lib/hooks";
import { sleep } from "@/lib/utils";

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
  onClose: (force?: boolean) => void;
}

export const DialogContext = createContext<DialogContextProps>(undefined!);

export interface DialogContextType {}

export const useDialog = () => {
  const context = useContext(DialogContext);
  if (context === undefined) {
    throw new Error("useDialog must be used within DialogProvider");
  }
  return context;
};

export const DialogProvider: FC<{ children: ReactNode; components: { name: string; Component: ComponentType<any> }[] }> = ({ children, components }) => {
  const queueRef = useRef(new PQueue({ concurrency: 1 }));

  const [dialogs, setDialogs] = useState<DialogProps[]>(
    components.map(({ name, Component }) => {
      const id = name.replace(/Modal$/, "").replace(/[A-Z]/g, (char, index) => (index !== 0 ? "-" : "") + char.toLowerCase());
      return { id, name, Component };
    }) as any
  );

  const updateDialog = (id: string, dialog: Partial<DialogProps>) => {
    return setDialogs((prevDialogs) => prevDialogs.map((d) => (d.id === id ? { ...d, ...dialog } : d)));
  };

  const context = useRef<DialogContextProps>({
    open: (id, props) => {
      return queueRef.current.add(async () => {
        updateDialog(id, {
          ...props,
          mounted: true,
          opened: true
        });
      });
    },
    close: async (id) => {
      return queueRef.current.add(async () => {
        updateDialog(id, { opened: false });
        await sleep();
        updateDialog(id, { mounted: false, opened: false });
      });
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

export const DIALOG_QUERY_KEY = "dialogId";

export const DialogRouter: FC = () => {
  const queueRef = useRef(new PQueue({ concurrency: 1 }));

  const dialog = useDialog();

  const router = useRouter();

  const pathname = usePathname();
  const searchParams = useSearchParams();
  const href = queryString.stringifyUrl({ url: pathname, query: Object.fromEntries(searchParams) });
  const prevHref = usePreviousValue(href) ?? queryString.parseUrl(href)?.url;

  useEffect(() => {
    queueRef.current.add(() =>
      (async () => {
        let prevUrl = queryString.parseUrl(prevHref);
        let url = queryString.parseUrl(href);

        const prevDialogId = prevUrl.query[DIALOG_QUERY_KEY] as string;
        if (prevDialogId) await dialog.close(prevDialogId);

        const dialogId = url.query[DIALOG_QUERY_KEY] as string;

        if (dialogId) {
          await dialog.open(dialogId, {
            onClose: async (force: boolean = true) => {
              // Close the dialog when it's closed.
              await dialog.close(dialogId);

              if (force) {
                router.replace(queryString.stringifyUrl({ url: url.url }));
              } else {
                router.replace(queryString.stringifyUrl(prevUrl));
              }
            }
          });
        }
      })()
    );
  }, [dialog, href, prevHref, router]);

  return <></>;
};
