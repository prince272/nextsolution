import { Text } from "react-native";
import { useTheme } from "react-native-paper";
import { SafeAreaView as View } from 'react-native-safe-area-context';

export default function Home() {
  const themeConfig = useTheme();
  return (
    <View className="flex flex-1 justify-center items-center">
      <Text>Home</Text>
    </View>
  );
}
