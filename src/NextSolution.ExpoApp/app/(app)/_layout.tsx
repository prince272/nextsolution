import { useAppStore } from "@/stores";
import { Redirect, Slot } from "expo-router";

export default function AppLayout() {
  const currentUser = useAppStore((state) => state.authentication.user);

  if (!currentUser) {
    return <Redirect href="/sign-in" />;
  }

  return <Slot />;
}
