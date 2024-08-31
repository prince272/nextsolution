import { Roboto as FontSans, Roboto_Mono as FontMono } from "next/font/google";

export const fontSans = FontSans({
  subsets: ["latin"],
  weight: ["400", "500", "700"],
  variable: "--font-sans"
});

export const fontMono = FontMono({
  subsets: ["latin"],
  weight: ["400", "700"],
  variable: "--font-mono"
});
