import { DarkTheme as NativeNavigationDarkTheme, DefaultTheme as NativeNavigationLightTheme } from "@react-navigation/native";
import { merge } from "lodash";
import { adaptNavigationTheme, configureFonts, MD3DarkTheme, MD3LightTheme } from "react-native-paper";
import { colors } from "./colors";

const fonts = configureFonts({
  config: {
  }
});


const { LightTheme: NavigationLightTheme, DarkTheme: NavigationDarkTheme } = adaptNavigationTheme({
  reactNavigationLight: NativeNavigationLightTheme,
  reactNavigationDark: NativeNavigationDarkTheme
});

const LightTheme = merge({}, NavigationLightTheme, MD3LightTheme, { colors: merge({}, MD3LightTheme.colors, colors.light), fonts });
const DarkTheme = merge({}, NavigationDarkTheme, MD3DarkTheme, { colors: merge({}, MD3DarkTheme.colors, colors.dark), fonts });

export { LightTheme, DarkTheme };
