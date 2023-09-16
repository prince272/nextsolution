import "@/styles/globals.css";

import { ReactNode } from "react";
import { Metadata } from "next";
import { cookies as cookieStore } from "next/headers";
import { fontSans } from "@/assets/fonts";

import { apiConfig } from "@/config/api";
import { appConfig } from "@/config/app";
import { ApiProvider } from "@/lib/api/client";
import { CookiesProvider } from "@/lib/cookies/client";
import { cn } from "@/lib/utils";
import { TopLoader } from "@/components/ui/top-loader";

import { AppProvider } from "../components/provider";

export const metadata: Metadata = {
  title: {
    default: appConfig.name,
    template: `%s - ${appConfig.name}`
  },
  description: appConfig.description,
  themeColor: [
    { media: "(prefers-color-scheme: light)", color: "white" },
    { media: "(prefers-color-scheme: dark)", color: "black" }
  ],
  icons: {
    icon: "/favicon.ico",
    shortcut: "/favicon-16x16.png",
    apple: "/apple-touch-icon.png"
  }
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en" className="h-full" suppressHydrationWarning>
      <head />
      <body className={cn("h-full min-h-screen bg-background font-sans text-foreground antialiased", fontSans.variable)}>
        <TopLoader />
        <CookiesProvider value={cookieStore().getAll()}>
          <ApiProvider config={apiConfig}>
            <AppProvider>{children}</AppProvider>
          </ApiProvider>
        </CookiesProvider>
      </body>
    </html>
  );
}
