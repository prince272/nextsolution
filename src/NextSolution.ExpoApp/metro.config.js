const { getDefaultConfig } = require("expo/metro-config");
const { withNativeWind } = require("nativewind/metro");

module.exports = (() => {
  // Get the default Expo Metro configuration
  const config = getDefaultConfig(__dirname);

  // Add NativeWind configuration
  const nativeWindConfig = withNativeWind(config, {
    input: "./global.css",
    configPath: "./tailwind.config.ts"
  });

  // Customize the configuration to handle SVG files
  const { transformer, resolver } = nativeWindConfig;

  nativeWindConfig.transformer = {
    ...transformer,
    babelTransformerPath: require.resolve("react-native-svg-transformer/expo")
  };
  nativeWindConfig.resolver = {
    ...resolver,
    assetExts: resolver.assetExts.filter((ext) => ext !== "svg"),
    sourceExts: [...resolver.sourceExts, "svg"]
  };

  return nativeWindConfig;
})();
