import { Appbar } from "@/components";
import { Stack } from "expo-router";
import { FormProvider, useForm } from "react-hook-form";
import { CreateAccountForm } from "@/services/types";

export default function Layout() {
  const form = useForm<CreateAccountForm>();
  return (
    <FormProvider {...form}>
      <Stack
        screenOptions={{
          animation: "fade_from_bottom",
          header: ({ navigation, back }) => {
            return (
              <Appbar.Header mode="small">
                {back ? <Appbar.BackAction onPress={navigation.goBack} /> : null}
              </Appbar.Header>
            );
          }
        }}
      >
        <Stack.Screen name="sign-up" />
        <Stack.Screen name="sign-up-enter-credentials" />
      </Stack>
    </FormProvider>
  );
}
