import { Link, Stack } from "expo-router";
import { Image, StyleSheet, Text, View } from "react-native";
import { useTheme } from "react-native-paper";

export default function SignIn() {
  const themeConfig = useTheme();

  return (
    <View style={{ ...styles.container, backgroundColor: themeConfig.colors.background }}>
      <Stack.Screen
        options={{
          title: "Sign In",
          headerShown: false
        }}
      />
      <Text>Sign In Screen</Text>
      <Link href="/">Go to Home</Link>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center"
  },
  image: {
    width: 50,
    height: 50
  }
});
