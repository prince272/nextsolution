import "@/styles/globals.css";

import type { Metadata } from "next";
import fonts from "@/assets/fonts";

import { ApiProvider, AppProvider } from "../components/providers";
import { cn } from "@nextui-org/react";

export const metadata: Metadata = {
  title: "Create Next App",
  description: "Generated by create next app"
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" className={cn(fonts.sansFont.variable)} suppressHydrationWarning>
      <body className="bg-background text-foreground">
        <ApiProvider
          config={{
            baseURL: process.env.SERVER_URL
          }}
        >
          <AppProvider>{children}</AppProvider>
        </ApiProvider>
      </body>
    </html>
  );
}
