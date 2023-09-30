"use client";

import { FC, useState } from "react";
import { usePathname, useRouter } from "next/navigation";
import { AddIcon, DockPanelLeftIcon } from "@/assets/icons";
import { Button } from "@nextui-org/button";
import queryString from "query-string";
import toast from "react-hot-toast";

import { useApi } from "@/lib/api/client";
import { getErrorMessage } from "@/lib/api/utils";

import { Chat, useChatBotStore } from ".";
import { Sheet, SheetBody, SheetContent, SheetFooter, SheetHeader } from "../../ui/sheet";
import { UserButton } from "../user-button";
import { ChatList } from "./chat-list";

export const ChatBotSidebar: FC = () => {
  const router = useRouter();
  const [loadingNewChat, setLoadingNewChat] = useState(false);
  const { sidebarOpened, openSidebar, closeSidebar, dispatchChats } = useChatBotStore();
  const api = useApi();
  const pathname = usePathname();

  const onNewChat = async () => {
    try {
      setLoadingNewChat(true);
      const newResponse = await api.post<Chat>(`/chats`, { title: "New Chat" });
      dispatchChats("add", newResponse.data);
      router.replace(`/chatbot/${newResponse.data.id}`);
      toast.success(`Chat created.`);
    } catch (error) {
      toast.error(getErrorMessage(error));
    } finally {
      setLoadingNewChat(false);
    }
  };

  return (
    <Sheet
      placement="left"
      className="text-white dark"
      classNames={{
        base: "max-w-[280px]",
        header: "px-2",
        body: "px-2",
        footer: "px-2"
      }}
      hideCloseButton
      isStatic={true}
      isOpen={sidebarOpened}
      onOpenChange={(opened) => {
        (opened ? openSidebar : closeSidebar)();
      }}
    >
      <SheetContent>
        <SheetHeader className="flex justify-between gap-2 pb-2">
          <Button
            as={"div"}
            href={queryString.stringifyUrl({ url: pathname, query: { dialogId: "new-chat" } })}
            className="h-11 justify-start px-4 !text-sm"
            variant="bordered"
            fullWidth
            startContent={<AddIcon className="h-4 w-4" />}
            isLoading={loadingNewChat}
            onPress={() => {
              onNewChat();
            }}
          >
            New Chat
          </Button>
          <Button className="h-11" variant="bordered" isIconOnly onPress={() => closeSidebar()}>
            <DockPanelLeftIcon className="h-5 w-5" type="outlined" />
          </Button>
        </SheetHeader>
        <SheetBody as={ChatList} className="p-0"></SheetBody>
        <SheetFooter>
          <UserButton />
        </SheetFooter>
      </SheetContent>
    </Sheet>
  );
};
