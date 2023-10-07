"use client";

import { FC } from "react";
import NextLink from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { AddIcon, DockPanelLeftIcon } from "@/assets/icons";
import { Button } from "@nextui-org/button";

import { useApi } from "@/lib/api/client";

import { useChatBotStore } from ".";
import { Sheet, SheetBody, SheetContent, SheetFooter, SheetHeader } from "../../ui/sheet";
import { UserButton } from "../user-button";
import { ChatList } from "./chat-list";

export const ChatSidebar: FC = () => {
  const router = useRouter();
  const { sidebarOpened, openSidebar, closeSidebar, dispatchChats } = useChatBotStore();
  const api = useApi();
  const pathname = usePathname();

  return (
    <Sheet
      placement="left"
      className="bg-background text-white dark"
      classNames={{
        base: "max-w-[280px]",
        header: "px-2",
        body: "px-2",
        footer: "px-2"
      }}
      hideCloseButton
      disableAnimation
      isStatic={true}
      isOpen={sidebarOpened}
      onOpenChange={(opened) => {
        (opened ? openSidebar : closeSidebar)();
      }}
    >
      <SheetContent tabIndex={null!}>
        <SheetHeader className="flex justify-between gap-2 pb-2">
          <Button as={NextLink} href="/chatbot" className="h-11 justify-start px-4 !text-sm" variant="bordered" fullWidth startContent={<AddIcon className="h-4 w-4" />}>
            New Chat
          </Button>
          <Button className="h-11" variant="bordered" isIconOnly onPress={() => closeSidebar()}>
            <DockPanelLeftIcon className="h-5 w-5" type="outlined" />
          </Button>
        </SheetHeader>
        <SheetBody as={ChatList} className="p-0"></SheetBody>
        <SheetFooter className="pt-2">
          <UserButton />
        </SheetFooter>
      </SheetContent>
    </Sheet>
  );
};
