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
    owner: "princeowusu",
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
      jsEngine: "hermes",
      package: "com.yourcompany.next_solution",
      versionCode: 1
    },
    web: {
      favicon: "./assets/images/favicon.png",
      bundler: "metro"
    },
    plugins: ["expo-font"],
    experiments: {
      typedRoutes: true
    },
    extra: {
      eas: {
        projectId: "6744e9b2-aab0-445e-bf30-eb4274386bd8"
      }
    }
  };
};

module.exports = config;
