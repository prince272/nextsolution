"use client";

import { ComponentType, FC, Fragment, PropsWithChildren, createContext, useContext, useRef } from "react";
import PQueue from "p-queue";

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

export const DialogContext = createContext<DialogContextProps>(undefined!);

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

export const DialogProvider: FC<PropsWithChildren<DialogContextType>> = ({ children, dialogs: _dialogs }) => {
  const queueRef = useRef(new PQueue({ concurrency: 1 }));

  const [dialogs, setDialogs] = useStateAsync<DialogProps[]>(_dialogs as any);

  const updateDialog = (id: string, dialog: Partial<DialogProps>) => {
    return setDialogs((prevDialogs) => prevDialogs.map((d) => (d.id === id ? { ...d, ...dialog } : d)));
  };

  const context = useRef<DialogContextProps>({
    open: (id, props) => {
      return queueRef.current.add(async () => {
        await updateDialog(id, {
          ...props,
          opened: true,
          mounted: true
        });
      });
    },
    close: async (id) => {
      return queueRef.current.add(async () => {
        await updateDialog(id, { opened: false, mounted: true });
        await new Promise((resolve) => setTimeout(resolve, 150));
        await updateDialog(id, { opened: false, mounted: false });
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
