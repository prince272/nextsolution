import { Appbar } from "@/components";
import { Stack } from "expo-router";
import { FormProvider, useForm } from "react-hook-form";
import { ResetPasswordForm } from "@/services/types";

export default function Layout() {
  const form = useForm<ResetPasswordForm>();
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
        <Stack.Screen name="reset-password/index" />
        <Stack.Screen name="reset-password/enter-verification-code" />
        <Stack.Screen name="reset-password/enter-new-password" />
      </Stack>
    </FormProvider>
  );
}
