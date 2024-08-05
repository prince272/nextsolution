import "../global.css";
import { useEffect } from "react";
import { SnackbarProvider } from "@/components";
import { useThemeConfig } from "@/configs/theme";
import { useHydration } from "@/states";
import { ThemeProvider } from "@react-navigation/native";
import { Slot, Stack } from "expo-router";
import * as SplashScreen from "expo-splash-screen";
import { StatusBar } from "expo-status-bar";
import { PaperProvider, Snackbar } from "react-native-paper";
import { initialWindowMetrics, SafeAreaProvider } from "react-native-safe-area-context";

export { ErrorBoundary } from "expo-router";

SplashScreen.preventAutoHideAsync();

export default function RootLayout() {
  const hydrated = useHydration();
  const { themeConfig, inverseTheme, fontsLoaded, fontsError } = useThemeConfig();

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
                    animation: "fade_from_bottom",
                    headerShown: false
                  }}
                />
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
