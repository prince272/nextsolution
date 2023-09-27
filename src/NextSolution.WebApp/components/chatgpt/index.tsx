"use client";

import { FC, ReactNode, useEffect } from "react";
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
import { ChatGPTSidebar } from "./sidebar";

export const ChatGPTLayout: FC<{ children: ReactNode }> = ({ children }) => {
  const router = useRouter();
  const api = useApi();
  const { loading } = useAppStore();
  const { sidebarOpened, toggleSidebar, dispatchChats, setChatsStatus } = useChatGPTStore();

  useUnauthenticated(() => {
    router.replace(queryString.stringifyUrl({ url: "/", query: { dialogId: "sign-in" } }));
  });

  const currentUser = useUser();

  const fetchData = async (offset: number) => {
    try {
      setChatsStatus({ action: "fetching" });
      const response = await api.get("/chats", { params: { offset: offset, limit: 25 } });
      dispatchChats("load", response.data);
      setChatsStatus({ action: "idle" });
    } catch (error) {
      setChatsStatus({ action: "idle", error });
    }
  };

  useEffect(() => {
    fetchData(0);
  }, []);

  return (
    <Loader loading={loading || !currentUser} className="relative flex min-h-screen flex-col">
      {currentUser && (
        <div className={cn("flex flex-1 items-start")}>
          <ChatGPTSidebar />
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

export interface ChatGPTState {
  sidebarOpened: boolean;
  chats: {
    items: Chat[];
    groupedItems: Dictionary<ChatGPTState["chats"]["items"]>;
    groupedKeys: string[];
    groupedCounts: number[];
    offset: number;
    limit: number;
    length: number;
    previous: number | null;
    next: number | null;
  };
  chatsStatus: { action: "idle" | "fetching"; error?: any };
}

export interface ChatGPTActions {
  openSidebar: () => void;
  closeSidebar: () => void;
  toggleSidebar: () => void;
  dispatchChats: (action: "add" | "update" | "remove" | "load", item: ChatGPTState["chats"] | Chat) => void;
  setChatsStatus: (status: ChatGPTState["chatsStatus"]) => void;
}

export const useChatGPTStore = zustand.create<ChatGPTState & ChatGPTActions>((set) => ({
  sidebarOpened: true,
  chats: {
    items: [],
    groupedItems: {},
    groupedKeys: [],
    groupedCounts: [],
    offset: 0,
    limit: 0,
    length: 0,
    previous: null,
    next: null
  },
  chatsStatus: { action: "fetching" },
  openSidebar: () => set((state) => ({ ...state, sidebarOpened: true })),
  closeSidebar: () => set((state) => ({ ...state, sidebarOpened: false })),
  toggleSidebar: () => set((state) => ({ ...state, sidebarOpened: !state.sidebarOpened })),
  dispatchChats: (action: "add" | "update" | "remove" | "load", item: ChatGPTState["chats"] | Chat) =>
    set((state) => {
      const chats = cloneDeep(action == "load" ? ({ ...item, items: [...state.chats.items, ...(item as ChatGPTState["chats"]).items] } as ChatGPTState["chats"]) : state.chats);

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
  setChatsStatus: (status: ChatGPTState["chatsStatus"]) => set((state) => ({ ...state, chatsStatus: status }))
}));
