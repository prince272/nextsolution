/**
 * @see https://prettier.io/docs/en/configuration.html
 * @type {import("prettier").Config}
 */
const config = {
  endOfLine: "auto",
  semi: true,
  singleQuote: false,
  tabWidth: 2,
  trailingComma: "none",
  printWidth: 100,
  plugins: ["@ianvs/prettier-plugin-sort-imports"],
  importOrder: [
    "^.+\\.(css|scss)$",
    "^(react/(.*)$)|^(react$)",
    "^(react-native/(.*)$)|^(react-native$)",
    "^(next/(.*)$)|^(next$)",
    "<THIRD_PARTY_MODULES>",
    "^@/configs/(.*)$",
    "^@/constants/(.*)$",
    "^@/types/(.*)$",
    "^@/lib/(.*)$",
    "^@/utils/(.*)$",
    "^@/states/(.*)$",
    "^@/services/(.*)$",
    "^@/hooks/(.*)$",
    "^@/components/ui/(.*)$",
    "^@/components/(.*)$",
    "^@/app/(.*)$",
    "^[./]"
  ],
  importOrderParserPlugins: ["typescript", "jsx", "decorators-legacy"]
};

module.exports = config;
