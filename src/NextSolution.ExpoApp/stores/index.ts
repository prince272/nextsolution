import { useEffect, useState } from "react";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { merge } from "lodash";
import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

export interface AppearanceState {
  theme: "light" | "dark" | "system";
  themeVersion: 2 | 3;
}

export interface AppearanceActions {
  setTheme: (theme: AppearanceState["theme"]) => void;
  setThemeVersion: (themeVersion: AppearanceState["themeVersion"]) => void;
}

export interface AppState {
  appearance: AppearanceState;
}

export interface AppStore extends AppState {
  appearance: AppearanceState & AppearanceActions;
}

const createAppStore = (initialState?: Partial<AppState>) => {
  const defaultState: Partial<AppState> = {
    appearance: {
      theme: "dark", // Default theme
      themeVersion: 3 // Default theme version
    }
  };

  const predefinedState = merge({}, defaultState, initialState) as AppState;

  return create<AppStore>()(
    persist(
      (set) => ({
        ...predefinedState,
        appearance: {
          ...predefinedState.appearance,
          setTheme: (theme) =>
            set((state) => ({
              appearance: {
                ...state.appearance,
                theme
              }
            })),
          setThemeVersion: (themeVersion) =>
            set((state) => ({
              appearance: {
                ...state.appearance,
                themeVersion
              }
            }))
        }
      }),
      {
        name: "NextSolution.Storage-1A114D3A52AA408FACFE89A437A9BCC4",
        storage: createJSONStorage(() => AsyncStorage),
        // zustand persist - actions inside nested object are undefined on rehydration
        // fix: https://github.com/pmndrs/zustand/issues/457
        merge: (persistedState, currentState) => {
          return merge({}, currentState, persistedState);
        }
      }
    )
  );
};

export const useAppStore = createAppStore();

export const useHydration = () => {
  const [hydrated, setHydrated] = useState(false);

  useEffect(() => {
    // Note: This is just in case you want to take into account manual rehydration.
    // You can remove the following line if you don't need it.
    const unsubHydrate = useAppStore.persist.onHydrate(() => setHydrated(false));

    const unsubFinishHydration = useAppStore.persist.onFinishHydration(() => setHydrated(true));

    setHydrated(useAppStore.persist.hasHydrated());

    return () => {
      unsubHydrate();
      unsubFinishHydration();
    };
  }, []);

  return hydrated;
};
