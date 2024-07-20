import { Appearance } from "react-native";
import { StateCreator } from "zustand";

export interface AppearanceState {
  theme: "light" | "dark" | "system";
  systemTheme: "light" | "dark";
  activeTheme: "light" | "dark";
  inverseTheme: "light" | "dark";
}

export interface AppearanceActions {
  setTheme: (theme: AppearanceState["theme"]) => void;
  setSystemTheme: (SystemTheme: AppearanceState["systemTheme"]) => void;
  addSystemThemeListener: () => { remove: () => void };
}

export interface AppearanceSlice {
  appearance: AppearanceState & AppearanceActions;
}

export const createAppearanceSlice: StateCreator<AppearanceSlice, [], [], AppearanceSlice> = (set, get) => {
  const initialTheme = "system" as AppearanceState["theme"];
  const initialSystemTheme = (Appearance.getColorScheme() ?? initialTheme == "system") ? "light" : initialTheme;

  const getActiveTheme = (theme: "light" | "dark" | "system", systemTheme: "light" | "dark") => (theme === "system" ? systemTheme : theme);

  const getInverseTheme = (theme: "light" | "dark" | "system", systemTheme: "light" | "dark") =>
    theme === "system" ? (systemTheme === "light" ? "dark" : "light") : theme === "light" ? "dark" : "light";

  return {
    appearance: {
      theme: initialTheme,
      systemTheme: initialSystemTheme,
      activeTheme: getActiveTheme(initialTheme, initialSystemTheme),
      inverseTheme: getInverseTheme(initialTheme, initialSystemTheme),
      setTheme: (theme) =>
        set((state) => ({
          appearance: {
            ...state.appearance,
            theme,
            activeTheme: getActiveTheme(theme, state.appearance.systemTheme),
            inverseTheme: getInverseTheme(theme, state.appearance.systemTheme)
          }
        })),
      setSystemTheme: (systemTheme) =>
        set((state) => ({
          appearance: {
            ...state.appearance,
            systemTheme,
            activeTheme: getActiveTheme(state.appearance.theme, systemTheme),
            inverseTheme: getInverseTheme(state.appearance.theme, systemTheme)
          }
        })),
      addSystemThemeListener: () => {
        const subscription = Appearance.addChangeListener(({ colorScheme: systemTheme }) => {
          if (systemTheme) get().appearance.setSystemTheme(systemTheme);
        });
        return {
          remove: subscription.remove
        };
      }
    }
  };
};
