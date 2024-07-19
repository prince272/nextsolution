import { ConfigContext, ExpoConfig } from "expo/config";

export default ({ config }: ConfigContext): Partial<ExpoConfig> => ({
  ...config
});
