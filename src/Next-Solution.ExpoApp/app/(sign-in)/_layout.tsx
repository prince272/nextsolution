import { Appbar } from "@/components";
import { Stack } from "expo-router";
import { FormProvider, useForm } from "react-hook-form";
import { SignInForm } from "@/services/types";

export default function Layout() {
  const form = useForm<SignInForm>();
  return (
    <FormProvider {...form}>
      <Stack
        screenOptions={{
          header: ({ navigation, back }) => {
            return (
              <Appbar.Header mode="small">
                {back ? <Appbar.BackAction onPress={navigation.goBack} /> : null}
              </Appbar.Header>
            );
          }
        }}
      >
        <Stack.Screen name="sign-in" />
      </Stack>
    </FormProvider>
  );
}
