/**
 * @param {import('expo/config').ConfigContext} context
 * @returns {import('expo/config').ExpoConfig}
 */
const config = (context) => {
  const { config } = context;
  return {
    ...config,
    name: "Next Solution",
    slug: "next-solution",
    version: "1.0.0",
    owner: "oprince15799",
    orientation: "portrait",
    icon: "./assets/images/icon.png",
    scheme: "next-solution",
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
    extra: {
      eas: {
        projectId: "9ecced41-d88e-488e-a37c-bb456b3f9286"
      }
    }
  };
};

module.exports = config;
