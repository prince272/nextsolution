import { useEffect, useMemo } from "react";
import { colors } from "@/configs/colors";
import { fonts } from "@/configs/fonts";
import { useAppStore } from "@/states";
import {
  DarkTheme as NativeNavigationDarkTheme,
  DefaultTheme as NativeNavigationLightTheme
} from "@react-navigation/native";
import { useFonts } from "expo-font";
import { merge } from "lodash";
import { adaptNavigationTheme, MD3DarkTheme, MD3LightTheme, MD3Theme } from "react-native-paper";

const { LightTheme: NavigationLightTheme, DarkTheme: NavigationDarkTheme } = adaptNavigationTheme({
  reactNavigationLight: NativeNavigationLightTheme,
  reactNavigationDark: NativeNavigationDarkTheme
});

const themes = {
  Light: merge({}, NavigationLightTheme, MD3LightTheme, {
    animation: { defaultAnimationDuration: 0 },
    colors: merge({}, MD3LightTheme.colors, colors.light),
    fonts
  } as MD3Theme),
  Dark: merge({}, NavigationDarkTheme, MD3DarkTheme, {
    animation: { defaultAnimationDuration: 0 },
    colors: merge({}, MD3DarkTheme.colors, colors.dark),
    fonts
  } as MD3Theme)
};

export function useThemeConfig() {
  const { activeTheme, inverseTheme, addSystemThemeChangeListener } = useAppStore(
    (state) => state.appearance
  );

  const themeConfig = useMemo(
    () => (activeTheme === "dark" ? themes.Dark : themes.Light),
    [activeTheme]
  );

  const [fontsLoaded, fontsError] = useFonts({
    "Roboto-Regular": require("@/assets/fonts/roboto/Roboto-Regular.ttf"),
    "Roboto-Medium": require("@/assets/fonts/roboto/Roboto-Medium.ttf"),
    "Roboto-Bold": require("@/assets/fonts/roboto/Roboto-Bold.ttf")
  });

  useEffect(() => {
    const systemThemeListener = addSystemThemeChangeListener();
    return () => systemThemeListener.remove();
  }, []);

  return { themeConfig, activeTheme, inverseTheme, fontsLoaded, fontsError };
}
