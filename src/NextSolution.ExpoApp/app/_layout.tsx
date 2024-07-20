import { useEffect, useMemo } from "react";
import FontAwesome from "@expo/vector-icons/FontAwesome";
import { DarkTheme as NativeNavigationDarkTheme, DefaultTheme as NativeNavigationLightTheme, ThemeProvider as NavigationThemeProvider } from "@react-navigation/native";
import { useFonts } from "expo-font";
import { Stack } from "expo-router";
import * as SplashScreen from "expo-splash-screen";
import { adaptNavigationTheme, MD3DarkTheme, MD3LightTheme, PaperProvider } from "react-native-paper";

import "react-native-reanimated";
import "../global.css";

import { Colors } from "@/constants/Colors";
import { useAppStore, useHydration } from "@/stores";
import { StatusBar } from "expo-status-bar";
import { merge } from "lodash";
import { SafeAreaProvider } from "react-native-safe-area-context";

export { ErrorBoundary } from "expo-router";

SplashScreen.preventAutoHideAsync();

export default function RootLayout() {
  const hydrated = useHydration();

  const [loaded, error] = useFonts({
    SpaceMono: require("../assets/fonts/SpaceMono-Regular.ttf"),
    ...FontAwesome.font
  });

  useEffect(() => {
    if (error) throw error;
  }, [error]);

  useEffect(() => {
    if (loaded && hydrated) {
      SplashScreen.hideAsync();
    }
  }, [loaded, hydrated]);

  if (!loaded || !hydrated) {
    return null;
  }

  return <Root />;
}

const { LightTheme: NavigationLightTheme, DarkTheme: NavigationDarkTheme } = adaptNavigationTheme({
  reactNavigationLight: NativeNavigationLightTheme,
  reactNavigationDark: NativeNavigationDarkTheme
});

const LightTheme = merge({}, NavigationLightTheme, { ...MD3LightTheme, colors: { ...MD3LightTheme.colors, ...Colors.light } });
const DarkTheme = merge({}, NavigationDarkTheme, { ...MD3DarkTheme, colors: { ...MD3DarkTheme.colors, ...Colors.dark } });

const Root = () => {
  const { activeTheme, inverseTheme, addSystemThemeListener } = useAppStore((state) => state.appearance);
  const themeConfig = useMemo(() => (activeTheme == "dark" ? DarkTheme : LightTheme), [activeTheme]);

  useEffect(() => {
    const systemThemeListener = addSystemThemeListener();
    return () => systemThemeListener.remove();
  }, []);

  return (
    <SafeAreaProvider>
      <PaperProvider theme={themeConfig}>
        <NavigationThemeProvider value={themeConfig}>
          <StatusBar style={inverseTheme} />
          <Stack />
        </NavigationThemeProvider>
      </PaperProvider>
    </SafeAreaProvider>
  );
};
