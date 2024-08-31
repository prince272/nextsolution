import "../styles/global.css";
import { useEffect, useMemo } from "react";
import { Appearance } from "react-native";
import { SnackbarProvider } from "@/components";
import { DarkThemeConfig, LightThemeConfig } from "@/configs/theme";
import { useAppearance, useHydration } from "@/states";
import { Theme } from "@/states/appearance";
import { ThemeProvider } from "@react-navigation/native";
import { Stack } from "expo-router";
import * as SplashScreen from "expo-splash-screen";
import { StatusBar } from "expo-status-bar";
import { PaperProvider, Snackbar } from "react-native-paper";
import { initialWindowMetrics, SafeAreaProvider } from "react-native-safe-area-context";
import { useFonts } from "expo-font";

export { ErrorBoundary } from "expo-router";

SplashScreen.preventAutoHideAsync();

export default function RootLayout() {
  const hydrated = useHydration();
  const { activeTheme, inverseTheme, setSystemTheme } = useAppearance();

  const themeConfig = useMemo(
    () => (activeTheme === "dark" ? DarkThemeConfig : LightThemeConfig),
    [activeTheme]
  );

  const [fontsLoaded, fontsError] = useFonts({
    "Roboto-Regular": require("@/assets/fonts/roboto/Roboto-Regular.ttf"),
    "Roboto-Medium": require("@/assets/fonts/roboto/Roboto-Medium.ttf"),
    "Roboto-Bold": require("@/assets/fonts/roboto/Roboto-Bold.ttf")
  });

  useEffect(() => {
    setSystemTheme(Appearance.getColorScheme() as Theme);
    const subscription = Appearance.addChangeListener(({ colorScheme: systemTheme }) => {
      if (systemTheme) setSystemTheme(systemTheme as Theme);
    });
    return () => subscription.remove();
  }, []);

  useEffect(() => {
    if (fontsError) throw fontsError;
  }, [fontsError]);

  useEffect(() => {
    if (hydrated && fontsLoaded) {
      SplashScreen.hideAsync();
    }
  }, [hydrated, fontsLoaded]);

  if (!hydrated || !fontsLoaded) {
    return null;
  }

  return (
    <SafeAreaProvider initialMetrics={initialWindowMetrics}>
      <ThemeProvider value={themeConfig}>
        <PaperProvider theme={themeConfig}>
          <SnackbarProvider>
            {({ messages: snackbarMessages, hide: hideSnackbar }) => (
              <>
                <Stack
                  screenOptions={{
                    headerShown: false
                  }}
                >
                  <Stack.Screen name="(app)" />
                  <Stack.Screen name="(sign-in)" />
                  <Stack.Screen name="(sign-up)" />
                  <Stack.Screen name="(reset-password)" />
                  <Stack.Screen name="+not-found" />
                </Stack>
                {snackbarMessages.map((snackbarMessage) => (
                  <Snackbar
                    key={snackbarMessage.key}
                    visible={snackbarMessage.visible}
                    duration={snackbarMessage.duration}
                    onDismiss={() => hideSnackbar(snackbarMessage.key)}
                  >
                    {snackbarMessage.content}
                  </Snackbar>
                ))}
              </>
            )}
          </SnackbarProvider>
        </PaperProvider>
      </ThemeProvider>
      <StatusBar style={inverseTheme} />
    </SafeAreaProvider>
  );
}
