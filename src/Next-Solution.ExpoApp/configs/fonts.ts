import * as Font from "expo-font";
import { configureFonts } from "react-native-paper";

const fonts = configureFonts({
  config: {
    displaySmall: {
      fontFamily: "Roboto-Regular",
      fontSize: 36,
      fontWeight: "400",
      letterSpacing: 0,
      lineHeight: 44
    },
    displayMedium: {
      fontFamily: "Roboto-Regular",
      fontSize: 45,
      fontWeight: "400",
      letterSpacing: 0,
      lineHeight: 52
    },
    displayLarge: {
      fontFamily: "Roboto-Regular",
      fontSize: 57,
      fontWeight: "400",
      letterSpacing: 0,
      lineHeight: 64
    },
    headlineSmall: {
      fontFamily: "Roboto-Regular",
      fontSize: 24,
      fontWeight: "400",
      letterSpacing: 0,
      lineHeight: 32
    },
    headlineMedium: {
      fontFamily: "Roboto-Regular",
      fontSize: 28,
      fontWeight: "400",
      letterSpacing: 0,
      lineHeight: 36
    },
    headlineLarge: {
      fontFamily: "Roboto-Regular",
      fontSize: 32,
      fontWeight: "400",
      letterSpacing: 0,
      lineHeight: 40
    },
    titleSmall: {
      fontFamily: "Roboto-Medium",
      fontSize: 14,
      fontWeight: "500",
      letterSpacing: 0.1,
      lineHeight: 20
    },
    titleMedium: {
      fontFamily: "Roboto-Medium",
      fontSize: 16,
      fontWeight: "500",
      letterSpacing: 0.15,
      lineHeight: 24
    },
    titleLarge: {
      fontFamily: "Roboto-Regular",
      fontSize: 22,
      fontWeight: "400",
      letterSpacing: 0,
      lineHeight: 28
    },
    labelSmall: {
      fontFamily: "Roboto-Medium",
      fontSize: 11,
      fontWeight: "500",
      letterSpacing: 0.5,
      lineHeight: 16
    },
    labelMedium: {
      fontFamily: "Roboto-Medium",
      fontSize: 12,
      fontWeight: "500",
      letterSpacing: 0.5,
      lineHeight: 16
    },
    labelLarge: {
      fontFamily: "Roboto-Medium",
      fontSize: 14,
      fontWeight: "500",
      letterSpacing: 0.1,
      lineHeight: 20
    },
    bodySmall: {
      fontFamily: "Roboto-Regular",
      fontSize: 12,
      fontWeight: "400",
      letterSpacing: 0.4,
      lineHeight: 18
    },
    bodyMedium: {
      fontFamily: "Roboto-Regular",
      fontSize: 14,
      fontWeight: "400",
      letterSpacing: 0.25,
      lineHeight: 20
    },
    bodyLarge: {
      fontFamily: "Roboto-Regular",
      fontSize: 16,
      fontWeight: "400",
      letterSpacing: 0.15,
      lineHeight: 24
    }
  }
});

export { fonts };
