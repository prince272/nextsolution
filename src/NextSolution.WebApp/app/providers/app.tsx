"use client";

import React, { useEffect, useState } from "react";
import { DialogProvider } from "@/ui/components/dialogs";
import { CircularProgress, NextUIProvider } from "@nextui-org/react";
import { ThemeProvider as NextThemesProvider } from "next-themes";

import { ExternalWindow } from "@/lib/external-window";

import { SignInModal } from "../signin";
import { SignUpModal } from "../signup";

export function AppProvider({ children }: { children: React.ReactNode }) {
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setTimeout(() => {
      setLoading(false);
      ExternalWindow.notify();
    }, 1500);
  }, []);

  return (
    <NextUIProvider>
      <NextThemesProvider attribute="class" defaultTheme="dark">
        {loading && (
          <div className="absolute z-[99999] flex h-full w-full flex-col items-center justify-center bg-background">
            <CircularProgress id="app-progress" aria-label="Loading..." />
          </div>
        )}
        <DialogProvider dialogs={[{ id: "signin", Component: SignInModal }, { id: "signup", Component: SignUpModal }]}>
          <div className={`${loading ? "hidden" : ""}`}> {children}</div>
        </DialogProvider>
      </NextThemesProvider>
    </NextUIProvider>
  );
}
