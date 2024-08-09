import { Stack, Unmatched } from "expo-router";

export default function NotFoundScreen() {
  return (
    <>
      <Stack.Screen options={{ title: "Oops!" }} />
      <Unmatched />
    </>
  );
}
