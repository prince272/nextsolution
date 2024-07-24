import { Redirect } from "expo-router";

export default function Home() {
  return <Redirect href="/(auth)/sign-in" />;
}
