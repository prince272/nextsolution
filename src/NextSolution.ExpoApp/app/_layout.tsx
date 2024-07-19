import { useEffect, useMemo, useState } from "react";
import FontAwesome from "@expo/vector-icons/FontAwesome";
import { DarkTheme as NativeNavigationDarkTheme, DefaultTheme as NativeNavigationLightTheme, ThemeProvider as NativeNavigationThemeProvider } from "@react-navigation/native";
import { useFonts } from "expo-font";
import { Slot } from "expo-router";
import * as SplashScreen from "expo-splash-screen";
import { adaptNavigationTheme, MD3DarkTheme, MD3LightTheme, PaperProvider } from "react-native-paper";

import "react-native-reanimated";

import { useHydration, useAppStore } from "@/stores";
import { Appearance } from "react-native";

export {
  // Catch any errors thrown by the Layout component.
  ErrorBoundary
} from "expo-router";

// Prevent the splash screen from auto-hiding before asset loading is complete.
SplashScreen.preventAutoHideAsync();

export default function RootLayout() {
  const [systemTheme, setSystemTheme] = useState(Appearance.getColorScheme());
  const userTheme = useAppStore((state) => state.appearance.theme);

  const { LightTheme: NavigationLightTheme, DarkTheme: NavigationDarkTheme } = adaptNavigationTheme({
    reactNavigationLight: NativeNavigationLightTheme,
    reactNavigationDark: NativeNavigationDarkTheme
  });

  const themeConfig = useMemo(
    () =>
      (userTheme === "system" ? systemTheme : userTheme) === "dark"
        ? {
            ...MD3DarkTheme,
            ...NavigationDarkTheme,
            colors: {
              ...MD3DarkTheme.colors,
              ...NavigationDarkTheme.colors
            }
          }
        : {
            ...MD3LightTheme,
            ...NavigationLightTheme,
            colors: { ...MD3LightTheme.colors, ...NavigationLightTheme.colors }
          },
    [systemTheme, userTheme]
  );

  useEffect(() => {
    const subscription = Appearance.addChangeListener(({ colorScheme }) => {
      setSystemTheme(colorScheme);
    });
    return () => subscription.remove();
  }, []);

  const hydrated = useHydration();

  const [loaded, error] = useFonts({
    SpaceMono: require("../assets/fonts/SpaceMono-Regular.ttf"),
    ...FontAwesome.font
  });

  // Expo Router uses Error Boundaries to catch errors in the navigation tree.
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

  return (
    <PaperProvider theme={themeConfig}>
      <NativeNavigationThemeProvider value={themeConfig}>
        <Slot />
      </NativeNavigationThemeProvider>
    </PaperProvider>
  );
}
