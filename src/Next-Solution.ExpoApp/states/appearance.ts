import { StateCreator } from "zustand";

export type Theme = "light" | "dark";

export interface AppearanceState {
  theme: Theme | "system";
  systemTheme: Theme;
  activeTheme: Theme;
  inverseTheme: Theme;
}

export interface AppearanceActions {
  setTheme: (theme: AppearanceState["theme"]) => void;
  setSystemTheme: (systemTheme: Theme) => void;
}

export interface AppearanceSlice {
  appearance: AppearanceState & AppearanceActions;
}

export const createAppearanceSlice: StateCreator<AppearanceSlice, [], [], AppearanceSlice> = (
  set,
  get
) => {
  const getActiveTheme = (theme: Theme | "system", systemTheme: Theme): Theme =>
    theme === "system" ? systemTheme : theme;

  const getInverseTheme = (theme: Theme | "system", systemTheme: Theme): Theme =>
    theme === "system"
      ? systemTheme === "light"
        ? "dark"
        : "light"
      : theme === "light"
        ? "dark"
        : "light";

  const initialTheme = "system" as AppearanceState["theme"];
  const initialSystemTheme = initialTheme === "system" ? "light" : initialTheme;
  const initialActiveTheme = getActiveTheme(initialTheme, initialSystemTheme);
  const initialInverseTheme = getInverseTheme(initialTheme, initialSystemTheme);

  return {
    appearance: {
      theme: initialTheme,
      systemTheme: initialSystemTheme,
      activeTheme: initialActiveTheme,
      inverseTheme: initialInverseTheme,
      setTheme: (theme) => {
        set((state) => ({
          appearance: {
            ...state.appearance,
            theme,
            activeTheme: getActiveTheme(theme, state.appearance.systemTheme),
            inverseTheme: getInverseTheme(theme, state.appearance.systemTheme)
          }
        }));
      },
      setSystemTheme: (systemTheme) => {
        set((state) => ({
          appearance: {
            ...state.appearance,
            systemTheme: systemTheme,
            activeTheme: getActiveTheme(state.appearance.theme, systemTheme),
            inverseTheme: getInverseTheme(state.appearance.theme, systemTheme)
          }
        }));
      }
    }
  };
};
