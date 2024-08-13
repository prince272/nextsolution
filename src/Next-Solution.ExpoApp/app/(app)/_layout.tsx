import { Text } from "react-native";
import { useAuthentication } from "@/states";
import { Redirect, Stack } from "expo-router";

export default function Layout() {
  const { user: currentUser } = useAuthentication();

  if (!currentUser) {
    return <Redirect href="/sign-in" />;
  }

  return <Stack />;
}
