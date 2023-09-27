"use client";

import { ComponentProps, ComponentPropsWithoutRef, FC, forwardRef, Fragment } from "react";
import NextLink from "next/link";
import { usePathname } from "next/navigation";
import { AddIcon, CommentIcon, DeleteIcon, DockPanelLeftIcon, EditIcon } from "@/assets/icons";
import { Render } from "@/ui/misc/render";
import { Button } from "@nextui-org/button";
import { Spinner } from "@nextui-org/spinner";
import queryString from "query-string";
import { GroupedVirtuoso } from "react-virtuoso";

import { useApi } from "@/lib/api/client";
import { getApiErrorMessage } from "@/lib/api/utils";
import { cn } from "@/lib/utils";

import { useChatGPTStore } from ".";
import { Sheet, SheetBody, SheetContent, SheetFooter, SheetHeader } from "../../ui/sheet";
import { UserButton } from "../user-button";

export const ChatGPTSidebar: FC = () => {
  const { sidebarOpened, openSidebar, closeSidebar } = useChatGPTStore();
  const pathname = usePathname();

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
            as={NextLink}
            href={queryString.stringifyUrl({ url: pathname, query: { dialogId: "new-chat" } })}
            className="h-11 justify-start px-4 !text-sm"
            variant="bordered"
            fullWidth
            startContent={<AddIcon className="h-4 w-4" />}
          >
            New Chat
          </Button>
          <Button className="h-11" variant="bordered" isIconOnly onPress={() => closeSidebar()}>
            <DockPanelLeftIcon className="h-5 w-5" type="outlined" />
          </Button>
        </SheetHeader>
        <SheetBody as={ChatListBox} className="p-0"></SheetBody>
        <SheetFooter>
          <UserButton />
        </SheetFooter>
      </SheetContent>
    </Sheet>
  );
};

type ChatListBoxProps = ComponentProps<typeof GroupedVirtuoso> & {};

const ChatListBox: FC<ChatListBoxProps> = forwardRef((props, ref) => {
  const pathname = usePathname();
  const api = useApi();
  const { dispatchChats, chats, chatsStatus, setChatsStatus } = useChatGPTStore();
  const fetchData = async (offset: number) => {
    try {
      setChatsStatus({ action: "fetching" });
      const response = await api.get("/chats", { params: { offset: offset, limit: 25 } });
      dispatchChats("load", response.data as any);
      setChatsStatus({ action: "idle" });
    } catch (error) {
      setChatsStatus({ action: "idle", error });
    }
  };

  return (
    <GroupedVirtuoso
      groupCounts={chats.groupedCounts}
      groupContent={(index) => {
        const groupedKey = chats.groupedKeys[index];
        return <>{groupedKey}</>;
      }}
      endReached={() => {
        if (chats.next) fetchData(chats.next);
      }}
      itemContent={(index) => {
        const item = chats.items[index];
        return (
          <Button
            as="div"
            variant="light"
            className="group relative h-11 w-full justify-start px-2"
            startContent={<CommentIcon className="pointer-events-none h-5 w-5 flex-shrink-0 text-xl" type="outlined" />}
            endContent={
              <div className="ml-auto hidden items-center group-hover:flex">
                <Button
                  as={NextLink}
                  href={queryString.stringifyUrl({ url: pathname, query: { dialogId: "edit-chat", chatId: item.id } })}
                  size="sm"
                  isIconOnly
                  variant="light"
                  color="primary"
                >
                  <EditIcon className="h-5 w-5" type="outlined" />
                </Button>
                <Button
                  as={NextLink}
                  href={queryString.stringifyUrl({ url: pathname, query: { dialogId: "delete-chat", chatId: item.id } })}
                  size="sm"
                  isIconOnly
                  variant="light"
                  color="danger"
                >
                  <DeleteIcon className="h-5 w-5" type="outlined" />
                </Button>
              </div>
            }
          >
            <NextLink href={`/chatgpt`} className="stretched-link"></NextLink>
            <div className="truncate">{item.title}</div>
          </Button>
        );
      }}
      components={{
        ...ChatListBoxComponents,
        Footer: () => {
          return (
            <Render switch={chatsStatus.error ? "error" : chats.next ? "loading" : "blank"}>
              <div key="error" className="flex h-24 flex-col items-center justify-center space-y-2 text-center">
                <div className="text-sm text-foreground-500">{getApiErrorMessage(chatsStatus.error)}</div>
                <Button
                  variant="light"
                  color="primary"
                  onPress={() => {
                    fetchData(chats.offset);
                  }}
                >
                  Retry
                </Button>
              </div>
              <div key="loading" className="flex h-24 flex-col items-center justify-center space-y-2 text-center">
                <Spinner className="flex h-full flex-none items-center justify-center" size="md" aria-label="Loading..." />
              </div>
              <Fragment key="blank"></Fragment>
            </Render>
          );
        }
      }}
      {...props}
      ref={ref}
    />
  );
});
ChatListBox.displayName = "ChatListBox";

const ChatListBoxComponents = {
  // eslint-disable-next-line react/display-name
  List: forwardRef<HTMLDivElement, ComponentPropsWithoutRef<"div">>(({ className, ...props }, ref) => {
    return <div className={cn("space-y-1", className)} {...props} ref={ref} />;
  }),
  // eslint-disable-next-line react/display-name
  Group: forwardRef<HTMLDivElement, ComponentPropsWithoutRef<"div">>(({ className, ...props }, ref) => {
    return <div className={cn("bg-content1 py-2 pl-3 text-xs font-semibold text-primary", className)} {...props} ref={ref} />;
  }),
  // eslint-disable-next-line react/display-name
  Item: forwardRef<HTMLDivElement, ComponentPropsWithoutRef<"div">>(({ className, ...props }, ref) => {
    return <div className={cn("px-2", className)} {...props} ref={ref} />;
  })
};
