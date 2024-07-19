import FontAwesome from "@expo/vector-icons/FontAwesome";
import { useFonts } from "expo-font";
import { Slot } from "expo-router";
import * as SplashScreen from "expo-splash-screen";
import { useEffect, useMemo, useState } from "react";
import {
  ThemeProvider as NativeNavigationThemeProvider,
  DefaultTheme as NativeNavigationLightTheme,
  DarkTheme as NativeNavigationDarkTheme,
} from "@react-navigation/native";
import {
  MD3DarkTheme,
  MD3LightTheme,
  PaperProvider,
  adaptNavigationTheme,
} from "react-native-paper";
import "react-native-reanimated";
import { Appearance } from "react-native";
import { useAppStore } from "@/app-store";
import { useStore } from "@/hooks";

export {
  // Catch any errors thrown by the Layout component.
  ErrorBoundary,
} from "expo-router";

// Prevent the splash screen from auto-hiding before asset loading is complete.
SplashScreen.preventAutoHideAsync();

export default function RootLayout() {
  const [systemTheme, setSystemTheme] = useState(Appearance.getColorScheme());
  const userTheme = useAppStore((state) => state.appearance.theme);

  const { LightTheme: NavigationLightTheme, DarkTheme: NavigationDarkTheme } =
    adaptNavigationTheme({
      reactNavigationLight: NativeNavigationLightTheme,
      reactNavigationDark: NativeNavigationDarkTheme,
    });

  const themeConfiguration = useMemo(
    () =>
      (userTheme === "system" ? systemTheme : userTheme) === "dark"
        ? {
            ...MD3DarkTheme,
            ...NavigationDarkTheme,
            colors: {
              ...MD3DarkTheme.colors,
              ...NavigationDarkTheme.colors,
            },
          }
        : {
            ...MD3LightTheme,
            ...NavigationLightTheme,
            colors: { ...MD3LightTheme.colors, ...NavigationLightTheme.colors },
          },
    [systemTheme, userTheme]
  );

  useEffect(() => {
    const subscription = Appearance.addChangeListener(({ colorScheme }) => {
      setSystemTheme(colorScheme);
    });
    return () => subscription.remove();
  }, []);

  const [loaded, error] = useFonts({
    SpaceMono: require("../assets/fonts/SpaceMono-Regular.ttf"),
    ...FontAwesome.font,
  });

  // Expo Router uses Error Boundaries to catch errors in the navigation tree.
  useEffect(() => {
    if (error) throw error;
  }, [error]);

  useEffect(() => {
    if (loaded) {
      SplashScreen.hideAsync();
    }
  }, [loaded]);

  if (!loaded) {
    return null;
  }

  return (
    <PaperProvider theme={themeConfiguration}>
      <NativeNavigationThemeProvider value={themeConfiguration}>
        <Slot />
      </NativeNavigationThemeProvider>
    </PaperProvider>
  );
}
