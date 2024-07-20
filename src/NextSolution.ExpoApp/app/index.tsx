import { useAppStore } from "@/stores";
import { Link, Stack } from "expo-router";
import { Image, Pressable, StyleSheet, Text, View } from "react-native";
import { useTheme } from "react-native-paper";

function LogoTitle(props: any) {
  return <Image style={styles.image} source={{ uri: "https://reactnative.dev/img/tiny_logo.png" }} />;
}

export default function Home() {
  const themeConfig = useTheme();
  const [theme, setTheme] = useAppStore((state) => [state.appearance.theme, state.appearance.setTheme]);

  return (
    <View style={{ ...styles.container, backgroundColor: themeConfig.colors.background }}>
      <Stack.Screen
        options={{
          title: "My home",
          headerShown: false,
          headerTitle: (props) => <LogoTitle {...props} />
        }}
      />
      <Pressable
        onPress={() => {
          // togle between dark and light and system theme
          setTheme(theme === "dark" ? "light" : theme === "light" ? "system" : "dark");
        }}
      >
        <Text>Home Screen ({theme})</Text>
      </Pressable>
      <Link href="/sign-in">Go to Sign In Screen</Link>
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
