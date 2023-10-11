"use client";

import { FC, ReactNode } from "react";
import { usePathname, useRouter } from "next/navigation";
import { DockPanelLeftIcon } from "@/assets/icons";
import { Button } from "@nextui-org/button";
import queryString from "query-string";

import { useApi, useUnauthenticated, useUser } from "@/lib/api/client";
import { cn } from "@/lib/utils";

import { useChatBotStore } from ".";
import { Loader } from "../../ui/loader";
import { useAppStore } from "../provider";
import { ChatSidebar } from "./chat-sidebar";

export const ChatLayout: FC<{ children: ReactNode }> = ({ children }) => {
  const router = useRouter();
  const pathname = usePathname();
  const api = useApi();
  const { loading } = useAppStore();
  const { sidebarOpened, toggleSidebar, dispatchChats, setChatsStatus } = useChatBotStore();

  useUnauthenticated(() => {
    router.replace(queryString.stringifyUrl({ url: "/", query: { dialogId: "sign-in" } }));
  });

  const currentUser = useUser();

  return (
    <Loader loading={loading || !currentUser} className="relative flex min-h-screen flex-col">
      {currentUser && (
        <div className={cn("flex flex-1 items-start")}>
          <ChatSidebar />
          <div className="relative flex h-full w-full flex-col bg-default-50">
            <header className="sticky top-0 z-20 flex items-center justify-between bg-default-50 px-4 py-4">
              <div className="grid w-full grid-cols-3 items-center">
                <Button className={cn("h-11", sidebarOpened && "invisible")} variant="bordered" isIconOnly onPress={() => toggleSidebar()}>
                  <DockPanelLeftIcon className="h-5 w-5" type="outlined" />
                </Button>
                <div id="chat-title" className="text-center" />
              </div>
            </header>
            {children}
          </div>
        </div>
      )}
    </Loader>
  );
};
