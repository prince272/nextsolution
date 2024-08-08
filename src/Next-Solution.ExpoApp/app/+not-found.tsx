import { Text, View } from "@/components";
import { Stack } from "expo-router";

export default function NotFoundScreen() {
  return (
    <>
      <Stack.Screen options={{ title: "Oops!" }} />
      <View>
        <Text>You are lost, Kindly go back</Text>
      </View>
    </>
  );
}
