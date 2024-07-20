import { ConfigContext, ExpoConfig } from "expo/config";

export default ({ config }: ConfigContext): Partial<ExpoConfig> => ({
  ...config,
  name: "NextSolution",
  slug: "next-solution",
  version: "1.0.0",
  orientation: "portrait",
  icon: "./assets/images/icon.png",
  scheme: "nextsoln",
  userInterfaceStyle: "automatic",
  splash: {
    image: "./assets/images/splash.png",
    resizeMode: "contain",
    backgroundColor: "#ffffff"
  },
  ios: {
    supportsTablet: true
  },
  android: {
    adaptiveIcon: {
      foregroundImage: "./assets/images/adaptive-icon.png",
      backgroundColor: "#ffffff"
    },
    jsEngine: "hermes"
  },
  web: {
    bundler: "metro",
    output: "static",
    favicon: "./assets/images/favicon.png"
  },
  plugins: ["expo-router"],
  experiments: {
    typedRoutes: true
  }
});
