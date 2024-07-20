import { useAppStore } from "@/stores";
import { Link, Stack } from "expo-router";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { useTheme } from "react-native-paper";

export default function Home() {
  return (
    <View>
      <Stack.Screen
        options={{
          title: "My home",
          headerShown: false,
        }}
      />
    </View>
  );
}