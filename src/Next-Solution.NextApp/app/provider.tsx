"use client";

import { Toaster } from "@/components/toaster";
import { ModalController, ModalControllerProvider } from "@/modals";
import { SignInModal } from "@/modals/sign-in-modal";
import { SignUpModal } from "@/modals/sign-up-modal";
import { NextUIProvider } from "@nextui-org/system";
import { ThemeProvider as NextThemesProvider } from "next-themes";

export const AppProvider = ({ children }: { children: React.ReactNode }) => {
  return (
    <NextUIProvider>
      <NextThemesProvider attribute="class" defaultTheme="dark">
        <ModalControllerProvider>
          {children}
          <ModalController id="sign-in" modal={SignInModal} />
          <ModalController id="sign-up" modal={SignUpModal} />
        </ModalControllerProvider>
        <Toaster />
      </NextThemesProvider>
    </NextUIProvider>
  );
};
