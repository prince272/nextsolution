import { Roboto_Serif } from "next/font/google";

// If loading a variable font, you don't need to specify the font weight
const sansFont = Roboto_Serif({
  subsets: [],
  variable: "--font-sans",
  display: "swap"
});

const fonts = { sansFont };
export default fonts;
