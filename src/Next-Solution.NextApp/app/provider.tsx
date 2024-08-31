"use client";

import { ModalController, ModalControllerProvider } from "@/modals";
import { SignInMethodModal } from "@/modals/sign-in-method-modal";
import { SignInModal } from "@/modals/sign-in-modal";
import { SignUpModal } from "@/modals/sign-up-modal";
import { NextUIProvider } from "@nextui-org/system";
import { ThemeProvider as NextThemesProvider } from "next-themes";
import { Toaster } from "@/components/toaster";
import { useEffect } from "react";
import { WebBrowser } from "@/libs/web-browser";

export const AppProvider = ({ children }: { children: React.ReactNode }) => {

  useEffect(() => {
    WebBrowser.notify();
  }, []);

  return (
    <NextUIProvider>
      <NextThemesProvider attribute="class" defaultTheme="dark">
        <ModalControllerProvider>
          {children}
          <ModalController id="sign-in" modal={SignInModal} />
          <ModalController id="sign-in-method" modal={SignInMethodModal} />
          <ModalController id="sign-up" modal={SignUpModal} />
        </ModalControllerProvider>
        <Toaster />
      </NextThemesProvider>
    </NextUIProvider>
  );
};
