import { nextui, semanticColors } from "@nextui-org/theme";
import typography from '@tailwindcss/typography';

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./ui/**/*.{js,ts,jsx,tsx,mdx}",
    "./components/**/*.{js,ts,jsx,tsx,mdx}",
    "./assets/**/*.{js,ts,jsx,tsx,mdx}",
    "./app/**/*.{js,ts,jsx,tsx,mdx}",
    "./node_modules/@nextui-org/theme/dist/**/*.{js,ts,jsx,tsx}"
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ["var(--font-sans)"]
      },
      typography: ({ theme }) => ({
        light: {
          css: {
            '--tw-prose-body': semanticColors.light.default[800],
            '--tw-prose-headings': semanticColors.light.default[900],
            '--tw-prose-lead': semanticColors.light.default[700],
            '--tw-prose-links': semanticColors.light.default[900],
            '--tw-prose-bold': semanticColors.light.default[900],
            '--tw-prose-counters': semanticColors.light.default[600],
            '--tw-prose-bullets': semanticColors.light.default[400],
            '--tw-prose-hr': semanticColors.light.default[300],
            '--tw-prose-quotes': semanticColors.light.default[900],
            '--tw-prose-quote-borders': semanticColors.light.default[300],
            '--tw-prose-captions': semanticColors.light.default[700],
            '--tw-prose-code': semanticColors.light.default[900],
            '--tw-prose-pre-code': semanticColors.light.default[100],
            '--tw-prose-pre-bg': semanticColors.light.default[900],
            '--tw-prose-th-borders': semanticColors.light.default[300],
            '--tw-prose-td-borders': semanticColors.light.default[200],
            '--tw-prose-invert-body': semanticColors.light.default[200],
            '--tw-prose-invert-headings': theme('colors.white'),
            '--tw-prose-invert-lead': semanticColors.light.default[300],
            '--tw-prose-invert-links': theme('colors.white'),
            '--tw-prose-invert-bold': theme('colors.white'),
            '--tw-prose-invert-counters': semanticColors.light.default[400],
            '--tw-prose-invert-bullets': semanticColors.light.default[600],
            '--tw-prose-invert-hr': semanticColors.light.default[700],
            '--tw-prose-invert-quotes': semanticColors.light.default[100],
            '--tw-prose-invert-quote-borders': semanticColors.light.default[700],
            '--tw-prose-invert-captions': semanticColors.light.default[400],
            '--tw-prose-invert-code': theme('colors.white'),
            '--tw-prose-invert-pre-code': semanticColors.light.default[300],
            '--tw-prose-invert-pre-bg': 'rgb(0 0 0 / 50%)',
            '--tw-prose-invert-th-borders': semanticColors.light.default[600],
            '--tw-prose-invert-td-borders': semanticColors.light.default[700],
          },
        },
        dark: {
          css: {
            '--tw-prose-body': semanticColors.dark.default[800],
            '--tw-prose-headings': semanticColors.dark.default[900],
            '--tw-prose-lead': semanticColors.dark.default[700],
            '--tw-prose-links': semanticColors.dark.default[900],
            '--tw-prose-bold': semanticColors.dark.default[900],
            '--tw-prose-counters': semanticColors.dark.default[600],
            '--tw-prose-bullets': semanticColors.dark.default[400],
            '--tw-prose-hr': semanticColors.dark.default[300],
            '--tw-prose-quotes': semanticColors.dark.default[900],
            '--tw-prose-quote-borders': semanticColors.dark.default[300],
            '--tw-prose-captions': semanticColors.dark.default[700],
            '--tw-prose-code': semanticColors.dark.default[900],
            '--tw-prose-pre-code': semanticColors.dark.default[100],
            '--tw-prose-pre-bg': semanticColors.dark.default[900],
            '--tw-prose-th-borders': semanticColors.dark.default[300],
            '--tw-prose-td-borders': semanticColors.dark.default[200],
            '--tw-prose-invert-body': semanticColors.dark.default[200],
            '--tw-prose-invert-headings': theme('colors.blank'),
            '--tw-prose-invert-lead': semanticColors.dark.default[300],
            '--tw-prose-invert-links': theme('colors.black'),
            '--tw-prose-invert-bold': theme('colors.black'),
            '--tw-prose-invert-counters': semanticColors.dark.default[400],
            '--tw-prose-invert-bullets': semanticColors.dark.default[600],
            '--tw-prose-invert-hr': semanticColors.dark.default[700],
            '--tw-prose-invert-quotes': semanticColors.dark.default[100],
            '--tw-prose-invert-quote-borders': semanticColors.dark.default[700],
            '--tw-prose-invert-captions': semanticColors.dark.default[400],
            '--tw-prose-invert-code': theme('colors.black'),
            '--tw-prose-invert-pre-code': semanticColors.dark.default[300],
            '--tw-prose-invert-pre-bg': 'rgb(255 255 255 / 50%)',
            '--tw-prose-invert-th-borders': semanticColors.dark.default[600],
            '--tw-prose-invert-td-borders': semanticColors.dark.default[700],
          },
        },
      })
    }
  },
  darkMode: "class",
  plugins: [
    nextui({
      layout: {
        borderWidth: {
          small: "1px", // border-small
          medium: "1px", // border-medium
          large: "2px" // border-large
        }
      }
    }),
    typography(),
  ]
};
