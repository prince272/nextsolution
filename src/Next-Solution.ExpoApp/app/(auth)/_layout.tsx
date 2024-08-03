import { getHeaderTitle } from "@react-navigation/elements";
import { NativeStackHeaderProps } from "@react-navigation/native-stack";
import { Stack } from "expo-router";
import { Appbar } from "react-native-paper";

export default function Layout() {
  return (
    <Stack
      screenOptions={{
        header: (props) => <Header {...props as any} />,
        animation:"fade_from_bottom"
      }}
    >
      <Stack.Screen name="sign-in" options={{ title: "", headerShown: true }} />
      <Stack.Screen name="sign-in-form" options={{ title: "", headerShown: true }} />
      <Stack.Screen name="sign-up-form" options={{ title: "", headerShown: true }} />
    </Stack>
  );
}

function Header({ navigation, route, options, back }: NativeStackHeaderProps) {
  const title = getHeaderTitle(options, route.name);

  return (
    <Appbar.Header mode="small">
      {back ? <Appbar.BackAction onPress={navigation.goBack} /> : null}
      <Appbar.Content title={title} />
    </Appbar.Header>
  );
}
