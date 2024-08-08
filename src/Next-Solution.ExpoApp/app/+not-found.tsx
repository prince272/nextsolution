import { Button, Text, View } from "@/components";
import { Stack, useRouter } from "expo-router";

export default function NotFoundScreen() {
  const router = useRouter();
  return (
    <>
      <Stack.Screen options={{ title: "Oops!" }} />
      <View safeArea>
        <Button
          onPress={() => {
            router.push("/sign-up");
          }}
        >
          /sign-up
        </Button>
        <Button
          onPress={() => {
            router.push("/sign-up-enter-credentials");
          }}
        >
          /sign-up-enter-credentials
        </Button>
        <Button
          onPress={() => {
            router.push("/");
          }}
        >
          /home
        </Button>
      </View>
    </>
  );
}
