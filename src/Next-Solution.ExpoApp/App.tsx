import "./global.css"
import { StatusBar } from 'expo-status-bar';
import { Text, View } from 'react-native';

export default function App() {
  return (
      <View className="flex flex-1 items-center justify-center">
        <Text className="text-base">Open up App.tsx to start working on your app!</Text>
        <StatusBar style="auto" />
      </View>
  );
}