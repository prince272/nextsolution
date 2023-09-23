import { create } from "zustand";

export type AppState = {
  loading: boolean;
};

export type AppActions = {
  startLoading: () => void;
  completeLoading: () => void;
};

export const useAppStore = create<AppState & AppActions>((set) => ({
  loading: true,
  startLoading: () => set((state) => ({ ...state, loading: true })),
  completeLoading: () => set((state) => ({ ...state, loading: false })),
}));
