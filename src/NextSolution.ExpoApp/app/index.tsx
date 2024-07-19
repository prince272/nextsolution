import { useAppStore } from "@/stores";
import { View, StyleSheet, Appearance } from "react-native";
import { Button, useTheme } from "react-native-paper";

const HomeScreen = () => {
  const [theme, setTheme] = useAppStore((state) => [state.appearance.theme, state.appearance.setTheme]);
  const themeConfig = useTheme();  

  return (
    <View style={{ backgroundColor: themeConfig.colors.background, ...styles.container  }}>
      <Button
        icon="camera"
        mode="contained"
        onPress={() => {
            console.log("Pressed");
            // Toogle between light and dark and system theme
            setTheme(theme === "dark" ? "light" : theme === "light" ? "system" : "dark");
        }}
      >
        {theme}
      </Button>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
  },
});

export default HomeScreen;
