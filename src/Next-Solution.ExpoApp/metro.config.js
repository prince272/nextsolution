const { getDefaultConfig } = require("expo/metro-config");
const { withNativeWind } = require("nativewind/metro");

module.exports = (() => {
  const baseConfig = getDefaultConfig(__dirname);

  // Integrate NativeWind configuration
  const nativeWindConfig = withNativeWind(baseConfig, { input: "./styles/global.css" });

  const { transformer, resolver } = nativeWindConfig;

  // Add SVG transformer configuration
  const config = {
    ...nativeWindConfig,
    transformer: {
      ...transformer,
      babelTransformerPath: require.resolve("react-native-svg-transformer/expo")
    },
    resolver: {
      ...resolver,
      assetExts: resolver.assetExts.filter((ext) => ext !== "svg"),
      sourceExts: [...resolver.sourceExts, "svg"]
    }
  };

  return config;
})();
