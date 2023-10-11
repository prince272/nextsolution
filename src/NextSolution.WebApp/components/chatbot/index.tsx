"use client";

import dayjs from "dayjs";
import { cloneDeep, Dictionary, groupBy } from "lodash";
import * as zustand from "zustand";

export interface Chat {
  id: string;
  title: string;
  createdAt: string;
  updatedAt: string;
}

export interface ChatMessage {
  parentId: string;
  id: string;
  role: string;
  content: string;
  createdAt: string;
  updatedAt: string;
  childMessages: ChatMessage[];
}

export interface ChatStream {
  chatId: string;
  chatTitle: string;
  user: ChatMessage;
  assistant: ChatMessage
}

export interface ChatState {
  sidebarOpened: boolean;
  chats: {
    items: Chat[];
    groupedItems: Dictionary<ChatState["chats"]["items"]>;
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

export interface ChatActions {
  openSidebar: () => void;
  closeSidebar: () => void;
  toggleSidebar: () => void;
  dispatchChats: (action: "add" | "update" | "remove" | "removeAll" | "load", item?: ChatState["chats"] | Chat) => void;
  setChatsStatus: (status: ChatState["chatsStatus"]) => void;
}

const initialChatState: ChatState = {
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
  chatsStatus: { action: "loading" }
};

export const useChatBotStore = zustand.create<ChatState & ChatActions>((set) => ({
  ...initialChatState,
  openSidebar: () => set((state) => ({ ...state, sidebarOpened: true })),
  closeSidebar: () => set((state) => ({ ...state, sidebarOpened: false })),
  toggleSidebar: () => set((state) => ({ ...state, sidebarOpened: !state.sidebarOpened })),
  dispatchChats: (action: "add" | "update" | "remove" | "removeAll" | "load", item?: ChatState["chats"] | Chat) =>
    set((state) => {
      const chats = cloneDeep(
        action == "load"
          ? ({ ...item, items: [...state.chats.items, ...(item as ChatState["chats"]).items] } as ChatState["chats"])
          : action != "removeAll"
          ? state.chats
          : initialChatState.chats
      );

      chats.items = {
        add: [...chats.items, item as Chat],
        update: [...chats.items].map((x) => (x.id == (item as Chat)?.id ? (item as Chat) : x)),
        remove: [...chats.items].filter((x) => x.id != (item as Chat)?.id),
        removeAll: chats.items,
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
  setChatsStatus: (status: ChatState["chatsStatus"]) => set((state) => ({ ...state, chatsStatus: status }))
}));
