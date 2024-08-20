import { Text, View, Button } from "@/components";
import { useAuthentication } from "@/states";

export default function Screen() {
  const { user: currentUser, clearUser } = useAuthentication();

  const handleLogout = () => {
    clearUser();
  };

  return (
    <View safeArea className="flex-1 items-center justify-center">
      <Text className="text-center text-lg mb-4">
        {currentUser ? `I have logged in as ${currentUser.firstName}` : "No user logged in"}
      </Text>
      <Button onPress={handleLogout} className="mt-4">
        Sign Out
      </Button>
    </View>
  );
}
