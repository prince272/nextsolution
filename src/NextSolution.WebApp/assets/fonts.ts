import { Inter } from "next/font/google";

// If loading a variable font, you don't need to specify the font weight
const sansFont = Inter({
  subsets: [],
  variable: "--font-sans",
  display: "swap",
  weight: ["400", "500", "600", "700", "800"]
});

const fonts = { sansFont };
export default fonts;
