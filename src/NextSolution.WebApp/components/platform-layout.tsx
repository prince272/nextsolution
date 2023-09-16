"use client";

import { FC, ReactNode, useEffect } from "react";

import { useUser } from "@/lib/api/client";

import { PlatformSidebar } from "./platform-sidebar";
import { useApp } from "./provider";
import { Loader } from "./ui/loader";

export const PlatformLayout: FC<{ children: ReactNode }> = ({ children }) => {
  const app = useApp();
  app.authenticate();

  const currentUser = useUser();

  return (
    <Loader loading={app.loading || !currentUser} className="relative flex min-h-screen flex-col">
      {currentUser && (
        <div className="flex flex-1 items-start">
          <PlatformSidebar />
          <main className="container mx-auto px-6 pt-16" onClick={() => app.sidebar.toggle()}>
            {children}
            <footer className="flex w-full items-center justify-center py-3"></footer>
          </main>
        </div>
      )}
    </Loader>
  );
};
