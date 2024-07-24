// @ts-expect-error - no types
import nativewind from "nativewind/preset";
import { hairlineWidth } from "nativewind/theme";
import { colors, convertToCssVariables } from "./configs/colors";

export default {
  darkMode: "media",
  content: ["./app/**/*.{ts,tsx}", "./components/**/*.{ts,tsx}", "./screens/**/*.{ts,tsx}"],
  presets: [nativewind],
  theme: {
    extend: {
      colors: convertToCssVariables(colors.light),
      borderWidth: {
        hairline: hairlineWidth()
      }
    }
  },
  plugins: [
    ({ addBase }: { addBase: Function }) =>
      addBase({
        ":root": convertToCssVariables(colors.light, "--color-"),
        "@media (prefers-color-scheme: dark)": { ":root": convertToCssVariables(colors.dark, "--color-") }
      })
  ]
} satisfies import("tailwindcss").Config;
