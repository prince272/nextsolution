"use client";

import { FC, ReactNode, useEffect } from "react";
import { DockPanelLeftIcon } from "@/assets/icons";
import { Button } from "@nextui-org/button";
import queryString from "query-string";

import { useAuthentication, useUser } from "@/lib/api/client";
import { cn } from "@/lib/utils";

import { Loader } from "../../ui/loader";
import { useAppStore } from "../state";
import { ChatSidebar } from "./sidebar";
import { useChatGPT } from "./state";
import { useRouter } from "next/navigation";

export const ChatLayout: FC<{ children: ReactNode }> = ({ children }) => {
  const router = useRouter();
  const app = useAppStore();
  const chatgpt = useChatGPT();

  useAuthentication(() => {
    router.replace(queryString.stringifyUrl({ url: "/", query: { dialogId: "sign-in" } }));
  });

  const { sidebar } = chatgpt;
  const currentUser = useUser();

  return (
    <Loader loading={app.loading || !currentUser} className="relative flex min-h-screen flex-col">
      {currentUser && (
        <div className="flex flex-1 items-start">
          <ChatSidebar />
          <main className="container mx-auto px-6 pt-16">
            <div className={cn("absolute left-2 top-4", sidebar.opened ? "hidden" : "md:inline-block")}>
              <Button className="h-11" variant="bordered" isIconOnly onPress={() => sidebar.toggle()}>
                <DockPanelLeftIcon className="h-6 w-6" type="outlined" />
              </Button>
            </div>
            {children}
            <footer className="flex w-full items-center justify-center py-3"></footer>
          </main>
        </div>
      )}
    </Loader>
  );
};
