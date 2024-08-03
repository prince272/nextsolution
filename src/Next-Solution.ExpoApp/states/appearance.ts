import { Appearance } from "react-native";
import { StateCreator } from "zustand";

export type Theme = "light" | "dark";

export interface AppearanceState {
  theme: Theme | "system";
  systemScheme: Theme;
  activeTheme: Theme;
  inverseTheme: Theme;
}

export interface AppearanceActions {
  setTheme: (theme: AppearanceState["theme"]) => void;
  setSystemTheme: (systemTheme: Theme) => void;
  addSystemThemeChanged: () => { remove: () => void };
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
    theme === "system" ? (systemTheme === "light" ? "dark" : "light") : theme === "light" ? "dark" : "light";
  
  const initialTheme = "system" as AppearanceState["theme"];
  const initialSystemTheme = Appearance.getColorScheme() ?? (initialTheme === "system" ? "light" : initialTheme);
  const initialActiveTheme = getActiveTheme(initialTheme, initialSystemTheme);
  const initialInverseTheme = getInverseTheme(initialTheme, initialSystemTheme);

  return {
    appearance: {
      theme: initialTheme,
      systemScheme: initialSystemTheme,
      activeTheme: initialActiveTheme,
      inverseTheme: initialInverseTheme,
      setTheme: (theme) => {
        set((state) => ({
          appearance: {
            ...state.appearance,
            theme,
            activeTheme: getActiveTheme(theme, state.appearance.systemScheme),
            inverseTheme: getInverseTheme(theme, state.appearance.systemScheme),
          },
        }));
        Appearance.setColorScheme(theme === "system" ? get().appearance.systemScheme : theme);
      },
      setSystemTheme: (systemTheme) => {
        set((state) => ({
          appearance: {
            ...state.appearance,
            systemScheme: systemTheme,
            activeTheme: getActiveTheme(state.appearance.theme, systemTheme),
            inverseTheme: getInverseTheme(state.appearance.theme, systemTheme),
          },
        }));
      },
      addSystemThemeChanged: () => {
        const subscription = Appearance.addChangeListener(({ colorScheme: systemTheme }) => {
          if (systemTheme) get().appearance.setSystemTheme(systemTheme as Theme);
        });
        return {
          remove: subscription.remove,
        };
      },
    }
  };
};
