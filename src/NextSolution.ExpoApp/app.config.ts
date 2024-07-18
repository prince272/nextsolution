import { ExpoConfig, ConfigContext } from "expo/config";

export default ({ config }: ConfigContext): Partial<ExpoConfig> => ({
  ...config
});
