import "./global.css";
import { useCallback, useEffect, useState } from "react";
import { Text, View } from "react-native";
import * as SplashScreen from "expo-splash-screen";
import { useHydration } from "./states";
import { AppProvider } from "./components/provider";

// Keep the splash screen visible while we fetch resources
SplashScreen.preventAutoHideAsync();

export default function App() {
  const [loaded, setLoaded] = useState(false);
  const hydrated = useHydration();

  useEffect(() => {
    async function prepare() {
      try {
        // Pre-load fonts, make any API calls you need to do here
        //await Font.loadAsync(FontAwesome.font);
        // Artificially delay for two seconds to simulate a slow loading
        // experience. Please remove this if you copy and paste the code!
        await new Promise((resolve) => setTimeout(resolve, 2000));
      } catch (e) {
        console.warn(e);
      } finally {
        // Tell the application to render
        setLoaded(true);
      }
    }

    prepare();
  }, []);

  const hideSplashScreen = useCallback(() => {
    async function hideSplashScreen() {
      if (loaded && hydrated) {
        // Hide the splash screen once the app is ready
        await SplashScreen.hideAsync();
      }
    }

    hideSplashScreen();
  }, [loaded, hydrated]);

  if (!loaded || !hydrated) {
    return null;
  }

  return (
    <AppProvider>
      <View className="flex flex-1 items-center justify-center" onLayout={hideSplashScreen}>
        <Text>Your app goes here</Text>
      </View>
    </AppProvider>
  );
}