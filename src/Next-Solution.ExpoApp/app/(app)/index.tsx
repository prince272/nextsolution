import { Text, View } from "@/components";
import { useAuthentication } from "@/states";

export default function Screen() {
  const { user: currentUser, clearUser } = useAuthentication();
  return (
    <View safeArea className="items-center justify-center">
      <Text
        onPress={() => {
          clearUser();
        }}
      >
        I have logged in to {currentUser?.firstName}
      </Text>
    </View>
  );
}
