import { Button, Text, View } from "@/components";
import { useAuthentication } from "@/states";

export default function Screen() {
  const { user: currentUser, clearUser } = useAuthentication();

  const handleLogout = () => {
    clearUser();
  };

  return (
    <View safeArea className="flex-1 items-center justify-center bg-gray-100 px-4">
      {currentUser ? (
        <View className="bg-white rounded-lg shadow-lg p-6 w-full max-w-md">
          <Text className="text-2xl font-semibold mb-4 text-center text-gray-800">
            User Profile
          </Text>
          <Text className="text-lg mb-2 text-gray-700">
            <Text className="font-bold">Name:</Text> {currentUser.firstName} {currentUser.lastName}
          </Text>
          <Text className="text-lg mb-2 text-gray-700">
            <Text className="font-bold">Username:</Text> {currentUser.userName}
          </Text>
          <Text className="text-lg mb-2 text-gray-700">
            <Text className="font-bold">Email:</Text> {currentUser.email || "No email provided"}
          </Text>
          <Text className="text-lg mb-2 text-gray-700">
            <Text className="font-bold">Phone:</Text>{" "}
            {currentUser.phoneNumber || "No phone number provided"}
          </Text>
          <Text className="text-lg mb-2 text-gray-700">
            <Text className="font-bold">Email Confirmed:</Text>{" "}
            {currentUser.emailConfirmed ? "Yes" : "No"}
          </Text>
          <Text className="text-lg mb-2 text-gray-700">
            <Text className="font-bold">Phone Confirmed:</Text>{" "}
            {currentUser.phoneNumberConfirmed ? "Yes" : "No"}
          </Text>
          <Text className="text-lg mb-4 text-gray-700">
            <Text className="font-bold">Roles:</Text> {currentUser.roles.join(", ")}
          </Text>
          <Text className="text-lg mb-4 text-gray-700">
            <Text className="font-bold">Password Configured:</Text>{" "}
            {currentUser.passwordConfigured ? "Yes" : "No"}
          </Text>
        </View>
      ) : (
        <Text className="text-center text-lg mb-4 text-gray-800">No user logged in</Text>
      )}
      <Button onPress={handleLogout} className="mt-6 rounded-full shadow-lg">
        Sign Out
      </Button>
    </View>
  );
}
