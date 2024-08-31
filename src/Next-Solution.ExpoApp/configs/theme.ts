import { colors } from "@/configs/colors";
import { fonts } from "@/configs/fonts";
import {
  DarkTheme as NativeNavigationDarkTheme,
  DefaultTheme as NativeNavigationLightTheme
} from "@react-navigation/native";
import { merge } from "lodash";
import { adaptNavigationTheme, MD3DarkTheme, MD3LightTheme, MD3Theme } from "react-native-paper";

const { LightTheme: NavigationLightTheme, DarkTheme: NavigationDarkTheme } = adaptNavigationTheme({
  reactNavigationLight: NativeNavigationLightTheme,
  reactNavigationDark: NativeNavigationDarkTheme
});

export const LightThemeConfig = merge({}, NavigationLightTheme, MD3LightTheme, {
  colors: merge({}, MD3LightTheme.colors, colors.light),
  fonts
});

export const DarkThemeConfig = merge({}, NavigationDarkTheme, MD3DarkTheme, {
  colors: merge({}, MD3DarkTheme.colors, colors.dark),
  fonts
} as MD3Theme);
