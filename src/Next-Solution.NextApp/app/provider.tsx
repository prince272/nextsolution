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
import { ResetPasswordModal } from "@/modals/reset-password";
import { useAppearance } from "@/states";

export const AppProvider = ({ children }: { children: React.ReactNode }) => {

  const { activeTheme, systemTheme } = useAppearance();

  useEffect(() => {
    WebBrowser.notify();
  }, []);

  return (
    <NextUIProvider>
      <NextThemesProvider attribute="class" forcedTheme={activeTheme} defaultTheme={systemTheme}>
        <ModalControllerProvider>
          {children}
          <ModalController id="sign-in" modal={SignInModal} />
          <ModalController id="sign-in-method" modal={SignInMethodModal} />
          <ModalController id="sign-up" modal={SignUpModal} />
          <ModalController id="reset-password" modal={ResetPasswordModal} />
        </ModalControllerProvider>
        <Toaster />
      </NextThemesProvider>
    </NextUIProvider>
  );
};
