"use client";

import { ComponentProps, ComponentPropsWithoutRef, FC, forwardRef, Fragment, useEffect } from "react";
import NextLink from "next/link";
import { useParams, usePathname } from "next/navigation";
import { CommentIcon, DeleteIcon, EditIcon } from "@/assets/icons";
import { Render } from "@/ui/misc/render";
import { Button } from "@nextui-org/button";
import { Spinner } from "@nextui-org/spinner";
import queryString from "query-string";
import { GroupedVirtuoso } from "react-virtuoso";

import { useApi } from "@/lib/api/client";
import { getErrorMessage } from "@/lib/api/utils";
import { cn } from "@/lib/utils";

import { useChatBotStore } from ".";

export type ChatListProps = ComponentProps<typeof GroupedVirtuoso> & {};

export const ChatList: FC<ChatListProps> = forwardRef((props, ref) => {
  const { chatId } = useParams() as { chatId: string[] };
  const pathname = usePathname();
  const api = useApi();
  const { dispatchChats, chats, chatsStatus, setChatsStatus } = useChatBotStore();
  const fetchData = async (offset: number) => {
    try {
      setChatsStatus({ action: "loading" });
      const response = await api.get("/chats", { params: { offset: offset, limit: 25 } });
      dispatchChats("load", response.data);
      setChatsStatus({ action: "idle" });
    } catch (error) {
      setChatsStatus({ action: "idle", error });
    }
  };

  useEffect(() => {
    if (chats.offset == null) fetchData(0);
  }, []);

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
            variant={item.id == chatId?.at(0) ? "flat" : "light"}
            className={cn("group relative h-11 w-full justify-start px-2")}
            startContent={<CommentIcon className="pointer-events-none h-5 w-5 flex-shrink-0 text-xl" type="outlined" />}
            endContent={
              <div className={cn("ml-auto items-center group-hover:flex", item.id == chatId?.at(0) ? "flex" : "hidden")}>
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
            <NextLink href={`/chatbot/${item.id}`} className="stretched-link"></NextLink>
            <div className="truncate">{item.title}</div>
          </Button>
        );
      }}
      components={{
        ...ChatListComponents,
        Footer: () => {
          return (
            <Render switch={chatsStatus.action == "loading" ? "loading" : chatsStatus.error ? "error" : "blank"}>
              <div key="error" className="flex h-24 flex-col items-center justify-center space-y-2 text-center">
                <div className="text-sm text-foreground-500">{getErrorMessage(chatsStatus.error)}</div>
                <Button
                  variant="light"
                  color="primary"
                  onPress={() => {
                    fetchData(chats.next || 0);
                  }}
                >
                  Reload
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
ChatList.displayName = "ChatList";

export const ChatListComponents = {
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
