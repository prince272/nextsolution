import { useEffect, useState } from "react";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { merge } from "lodash";
import { Appearance } from "react-native";
import { create, StoreApi, UseBoundStore } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

import { AppearanceSlice, createAppearanceSlice } from "./appearance";
import { AuthenticationSlice, createAuthenticationSlice } from "./authentication";

type WithSelectors<S> = S extends { getState: () => infer T } ? S & { state: { [K in keyof T]: () => T[K] } } : never;

const createSelectors = <S extends UseBoundStore<StoreApi<object>>>(_store: S) => {
  let store = _store as WithSelectors<typeof _store>;
  store.state = {};
  for (let k of Object.keys(store.getState())) {
    (store.state as any)[k] = () => store((s) => s[k as keyof typeof s]);
  }

  return store;
};

export const useAppStore = createSelectors(
  create<AppearanceSlice & AuthenticationSlice>()(
    persist(
      (...a) => ({
        ...createAppearanceSlice(...a),
        ...createAuthenticationSlice(...a)
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
  )
);

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
