/**
 * @param {import('expo/config').ConfigContext} context
 * @returns {import('expo/config').ExpoConfig}
 */
const config = (context) => {
  const { config } = context;
  return {
    ...config,
    name: "Next_Solution",
    slug: "next_solution",
    version: "1.0.0",
    orientation: "portrait",
    icon: "./assets/images/icon.png",
    scheme: "next_solution",
    userInterfaceStyle: "light",
    splash: {
      image: "./assets/images/splash.png",
      resizeMode: "contain",
      backgroundColor: "#ffffff"
    },
    ios: {
      supportsTablet: true,
      jsEngine: "hermes"
    },
    android: {
      adaptiveIcon: {
        foregroundImage: "./assets/images/adaptive-icon.png",
        backgroundColor: "#ffffff"
      },
      jsEngine: "hermes"
    },
    web: {
      favicon: "./assets/images/favicon.png",
      bundler: "metro"
    },
    plugins: ["expo-font"]
  };
};

module.exports = config;
