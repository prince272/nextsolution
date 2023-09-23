import { create } from "zustand";

export type ChatGPTState = {
  sidebar: {
    opened: boolean;
  };
};

export type ChatGPTActions = {
  sidebar: {
    open: () => void;
    close: () => void;
    toggle: () => void;
  };
};

export const useChatGPT = create<ChatGPTState & ChatGPTActions>((set) => ({
  sidebar: {
    opened: true,
    open: () => set((state) => ({ ...state, ...{ sidebar: { ...state.sidebar, opened: true } } })),
    close: () => set((state) => ({ ...state, ...{ sidebar: { ...state.sidebar, opened: false } } })),
    toggle: () => set((state) => ({ ...state, ...{ sidebar: { ...state.sidebar, opened: !state.sidebar?.opened } } }))
  }
}));
