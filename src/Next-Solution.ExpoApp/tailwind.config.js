const { colors, convertToCssVariables } = require("./configs/colors");

/** @type {import('tailwindcss').Config} */
const config = {
  // NOTE: Update this to include the paths to all of your component files.
  content: [
    "./app/**/*.{js,jsx,ts,tsx}",
    "./components/**/*.{js,jsx,ts,tsx}",
    "./screens/**/*.{js,jsx,ts,tsx}"
  ],
  presets: [require("nativewind/preset")],
  theme: {
    extend: {
      colors: convertToCssVariables(colors.light)
    }
  },
  plugins: [
    ({ addBase }) =>
      addBase({
        ":root": convertToCssVariables(colors.light, "--color-"),
        "@media (prefers-color-scheme: dark)": {
          ":root": convertToCssVariables(colors.dark, "--color-")
        }
      })
  ]
};

module.exports = config;
