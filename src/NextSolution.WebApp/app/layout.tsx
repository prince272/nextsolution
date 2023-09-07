import "@/styles/globals.css";

import { Metadata } from "next";
import { cookies as getCookies } from "next/headers";
import clsx from "clsx";

import { apiConfig } from "@/config/api";
import { fontSans } from "@/config/fonts";
import { siteConfig } from "@/config/site";
import { ApiProvider } from "@/lib/api/provider";
import { CookiesProvider } from "@/lib/cookies/provider";

import { AppProvider } from "./provider";
import { ReactNode } from "react";

export const metadata: Metadata = {
  title: {
    default: siteConfig.name,
    template: `%s - ${siteConfig.name}`
  },
  description: siteConfig.description,
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
  const cookies = getCookies();

  return (
    <html lang="en" suppressHydrationWarning>
      <head />
      <body className={clsx("min-h-screen bg-background font-sans text-foreground antialiased", fontSans.variable)}>
        <CookiesProvider value={cookies.getAll().map((cookie) => ({ name: cookie.name, value: cookie.value }))}>
          <ApiProvider config={apiConfig}>
            <AppProvider themeProps={{ attribute: "class", defaultTheme: "dark" }}>{children}</AppProvider>
          </ApiProvider>
        </CookiesProvider>
      </body>
    </html>
  );
}
