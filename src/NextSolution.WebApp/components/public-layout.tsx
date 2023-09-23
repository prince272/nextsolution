"use client";

import { FC, ReactNode } from "react";
import { Link } from "@nextui-org/link";

import { Loader } from "../ui/loader";
import { PublicNavbar } from "./public-navbar";
import { useAppStore } from "./state";

export const PublicLayout: FC<{ children: ReactNode }> = ({ children }) => {
  const app = useAppStore();

  return (
    <Loader loading={app.loading} className="relative flex min-h-screen flex-col">
      <PublicNavbar />
      <main className="container mx-auto max-w-7xl flex-grow px-6 pt-16">{children}</main>
      <footer className="flex w-full items-center justify-center py-3">
        <Link isExternal className="flex items-center gap-1 text-current" href="https://nextui-docs-v2.vercel.app?utm_source=next-app-template" title="nextui.org homepage">
          <span className="text-default-600">Powered by</span>
          <p className="text-primary">NextUI</p>
        </Link>
      </footer>
    </Loader>
  );
};
