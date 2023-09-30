"use client";

import { FC, ReactNode } from "react";
import { useRouter } from "next/navigation";
import { DockPanelLeftIcon } from "@/assets/icons";
import { Button } from "@nextui-org/button";
import dayjs from "dayjs";
import { cloneDeep, Dictionary, groupBy } from "lodash";
import queryString from "query-string";
import * as zustand from "zustand";

import { useApi, useUnauthenticated, useUser } from "@/lib/api/client";
import { cn } from "@/lib/utils";

import { Loader } from "../../ui/loader";
import { useAppStore } from "../provider";
import { ChatBotSidebar } from "./sidebar";

export const ChatBotLayout: FC<{ children: ReactNode }> = ({ children }) => {
  const router = useRouter();
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
          <ChatBotSidebar />
          <main className="container mx-auto px-6 pt-16">
            <div className={cn("absolute left-2 top-4", sidebarOpened ? "hidden" : "md:inline-block")}>
              <Button className="h-11" variant="bordered" isIconOnly onPress={() => toggleSidebar()}>
                <DockPanelLeftIcon className="h-5 w-5" type="outlined" />
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

export interface Chat {
  id: string;
  title: string;
  createdAt: string;
  updatedAt: string;
}

export interface ChatBotState {
  sidebarOpened: boolean;
  chats: {
    items: Chat[];
    groupedItems: Dictionary<ChatBotState["chats"]["items"]>;
    groupedKeys: string[];
    groupedCounts: number[];
    offset: number | null;
    limit: number;
    length: number;
    previous: number | null;
    next: number | null;
  };
  chatsStatus: { action: "idle" | "loading"; error?: any };
}

export interface ChatBotActions {
  openSidebar: () => void;
  closeSidebar: () => void;
  toggleSidebar: () => void;
  dispatchChats: (action: "add" | "update" | "remove" | "load", item: ChatBotState["chats"] | Chat) => void;
  setChatsStatus: (status: ChatBotState["chatsStatus"]) => void;
}

export const useChatBotStore = zustand.create<ChatBotState & ChatBotActions>((set) => ({
  sidebarOpened: true,
  chats: {
    items: [],
    groupedItems: {},
    groupedKeys: [],
    groupedCounts: [],
    offset: null,
    limit: 0,
    length: 0,
    previous: null,
    next: null
  },
  chatsStatus: { action: "loading" },
  openSidebar: () => set((state) => ({ ...state, sidebarOpened: true })),
  closeSidebar: () => set((state) => ({ ...state, sidebarOpened: false })),
  toggleSidebar: () => set((state) => ({ ...state, sidebarOpened: !state.sidebarOpened })),
  dispatchChats: (action: "add" | "update" | "remove" | "load", item: ChatBotState["chats"] | Chat) =>
    set((state) => {
      const chats = cloneDeep(action == "load" ? ({ ...item, items: [...state.chats.items, ...(item as ChatBotState["chats"]).items] } as ChatBotState["chats"]) : state.chats);

      chats.items = {
        add: [...chats.items, item as Chat],
        update: [...chats.items].map((x) => (x.id == (item as Chat).id ? (item as Chat) : x)),
        remove: [...chats.items].filter((x) => x.id != (item as Chat).id),
        load: chats.items
      }[action];

      chats.items = chats.items.filter((v, i, a) => a.findLastIndex((v2) => v2.id == v.id) === i); // Filter by id
      chats.items = chats.items.sort((a, b) => dayjs(b.updatedAt).diff(dayjs(a.updatedAt))); // Sort by updatedAt
      chats.groupedItems = groupBy(chats.items, (chat) => {
        const today = dayjs();
        const past = dayjs(chat.updatedAt);
        if (past.isSame(today, "day")) return "Today";
        else if (past.isSame(today.subtract(1, "day"), "day")) return "Yesterday";
        else if (past.isSame(today, "week")) return "Earlier this week";
        else if (past.isSame(today.subtract(1, "week"), "week")) return "Last week";
        else if (past.isSame(today, "month")) return "Earlier this month";
        else if (past.isSame(today.subtract(1, "month"), "month")) return "Last month";
        else if (past.isSame(today, "year")) return "Earlier this year";
        else if (past.isSame(today.subtract(1, "year"), "year")) return "Last year";
        else return `${today.diff(past, "year")} years ago`;
      });
      chats.groupedKeys = Object.keys(chats.groupedItems);
      chats.groupedCounts = Object.values(chats.groupedItems).map((items) => items.length);

      return {
        ...state,
        chats: chats
      };
    }),
  setChatsStatus: (status: ChatBotState["chatsStatus"]) => set((state) => ({ ...state, chatsStatus: status }))
}));
