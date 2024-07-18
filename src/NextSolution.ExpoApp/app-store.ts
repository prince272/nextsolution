import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";

export const useAppStore = create(
  persist((set, get) => ({}), {
    name: "NextSolution-1A114D3A52AA408FACFE89A437A9BCC4",
    storage: createJSONStorage(() => sessionStorage),
  })
);
