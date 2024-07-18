import { View, StyleSheet } from "react-native";
import { Button, useTheme } from "react-native-paper";

const HomeScreen = () => {
    const theme = useTheme();
    
  return (
    <View style={{ backgroundColor: theme.colors.background, ...styles.container  }}>
      <Button
        icon="camera"
        mode="contained"
        onPress={() => {
            console.log("Pressed")
        }}
      >
        Press me
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
