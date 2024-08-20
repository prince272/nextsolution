export const colors = {
  light: {
    primary: "rgb(0, 91, 189)",
    onPrimary: "rgb(255, 255, 255)",
    primaryContainer: "rgb(215, 226, 255)",
    onPrimaryContainer: "rgb(0, 26, 64)",
    secondary: "rgb(86, 94, 113)",
    onSecondary: "rgb(255, 255, 255)",
    secondaryContainer: "rgb(218, 226, 249)",
    onSecondaryContainer: "rgb(19, 28, 44)",
    tertiary: "rgb(125, 60, 188)",
    onTertiary: "rgb(255, 255, 255)",
    tertiaryContainer: "rgb(240, 219, 255)",
    onTertiaryContainer: "rgb(44, 0, 81)",
    error: "rgb(186, 26, 26)",
    onError: "rgb(255, 255, 255)",
    errorContainer: "rgb(255, 218, 214)",
    onErrorContainer: "rgb(65, 0, 2)",
    background: "rgb(254, 251, 255)",
    onBackground: "rgb(27, 27, 31)",
    surface: "rgb(254, 251, 255)",
    onSurface: "rgb(27, 27, 31)",
    surfaceVariant: "rgb(224, 226, 236)",
    onSurfaceVariant: "rgb(68, 71, 78)",
    outline: "rgb(116, 119, 127)",
    outlineVariant: "rgb(196, 198, 208)",
    shadow: "rgb(0, 0, 0)",
    scrim: "rgb(0, 0, 0)",
    inverseSurface: "rgb(47, 48, 51)",
    inverseOnSurface: "rgb(242, 240, 244)",
    inversePrimary: "rgb(172, 199, 255)",
    elevation: {
      level0: "transparent",
      level1: "rgb(241, 243, 252)",
      level2: "rgb(234, 238, 250)",
      level3: "rgb(226, 233, 248)",
      level4: "rgb(224, 232, 247)",
      level5: "rgb(218, 229, 246)"
    },
    surfaceDisabled: "rgba(27, 27, 31, 0.12)",
    onSurfaceDisabled: "rgba(27, 27, 31, 0.38)",
    backdrop: "rgba(45, 48, 56, 0.4)"
  },
  dark: {
    primary: "rgb(172, 199, 255)",
    onPrimary: "rgb(0, 47, 103)",
    primaryContainer: "rgb(0, 68, 145)",
    onPrimaryContainer: "rgb(215, 226, 255)",
    secondary: "rgb(190, 198, 220)",
    onSecondary: "rgb(40, 48, 65)",
    secondaryContainer: "rgb(63, 71, 89)",
    onSecondaryContainer: "rgb(218, 226, 249)",
    tertiary: "rgb(221, 184, 255)",
    onTertiary: "rgb(73, 0, 129)",
    tertiaryContainer: "rgb(100, 30, 163)",
    onTertiaryContainer: "rgb(240, 219, 255)",
    error: "rgb(255, 180, 171)",
    onError: "rgb(105, 0, 5)",
    errorContainer: "rgb(147, 0, 10)",
    onErrorContainer: "rgb(255, 180, 171)",
    background: "rgb(27, 27, 31)",
    onBackground: "rgb(227, 226, 230)",
    surface: "rgb(27, 27, 31)",
    onSurface: "rgb(227, 226, 230)",
    surfaceVariant: "rgb(68, 71, 78)",
    onSurfaceVariant: "rgb(196, 198, 208)",
    outline: "rgb(142, 144, 153)",
    outlineVariant: "rgb(68, 71, 78)",
    shadow: "rgb(0, 0, 0)",
    scrim: "rgb(0, 0, 0)",
    inverseSurface: "rgb(227, 226, 230)",
    inverseOnSurface: "rgb(47, 48, 51)",
    inversePrimary: "rgb(0, 91, 189)",
    elevation: {
      level0: "transparent",
      level1: "rgb(34, 36, 42)",
      level2: "rgb(39, 41, 49)",
      level3: "rgb(43, 46, 56)",
      level4: "rgb(44, 48, 58)",
      level5: "rgb(47, 51, 62)"
    },
    surfaceDisabled: "rgba(227, 226, 230, 0.12)",
    onSurfaceDisabled: "rgba(227, 226, 230, 0.38)",
    backdrop: "rgba(45, 48, 56, 0.4)"
  }
};

export type Colors = typeof colors;

export const convertToCssVariables = (
  obj: { [key: string]: any },
  prefix: string = "",
  suffix: string = ""
): { [key: string]: string } => {
  const convertedObject: { [key: string]: string } = {};

  const convert = (obj: { [key: string]: any }, parentKey: string = "") => {
    for (const [key, value] of Object.entries(obj)) {
      const newKey = parentKey
        ? `${parentKey}-${key.replace(/([A-Z])/g, "-$1").toLowerCase()}`
        : `${prefix}${key.replace(/([A-Z])/g, "-$1").toLowerCase()}${suffix}`;

      if (typeof value === "object" && !Array.isArray(value)) {
        convert(value, newKey);
      } else {
        convertedObject[newKey] = value;
      }
    }
  };

  convert(obj);
  return convertedObject;
};
