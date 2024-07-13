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
  printWidth: 180,
  plugins: ["prettier-plugin-tailwindcss", "@ianvs/prettier-plugin-sort-imports"],
  importOrder: [
    "^(react/(.*)$)|^(react$)",
    "^(next/(.*)$)|^(next$)",
    "<THIRD_PARTY_MODULES>",
    "",
    "^types$",
    "^@/types/(.*)$",
    "^@/config/(.*)$",
    "^@/lib/(.*)$",
    "^@/services/(.*)$",
    "^@/hooks/(.*)$",
    "^@/components/ui/(.*)$",
    "^@/components/(.*)$",
    "^@/styles/(.*)$",
    "^@/app/(.*)$",
    "",
    "^[./]"
  ],
  importOrderParserPlugins: ["typescript", "jsx", "decorators-legacy"]
};

module.exports = config;
