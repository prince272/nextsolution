import React, { ComponentProps, useState } from "react";
import FacebookColorIcon from "@/assets/icons/facebook-round-color-icon.svg";
import GoogleColorIcon from "@/assets/icons/google-color-icon.svg";
import { Button, Divider, Image, Text, TextInput, useSnackbar, View } from "@/components";
import { identityService } from "@/services";
import { router } from "expo-router";
import { cssInterop } from "nativewind";
import { useFormContext } from "react-hook-form";
import { TouchableRipple, useTheme } from "react-native-paper";
import { ValidationProblem } from "@/services/results";
import { CreateAccountForm } from "@/services/types";
import { useMemoizedValue } from "@/hooks";

export type AuthenticationMethodsScreenProps = ComponentProps<typeof View> & {};

const AuthScreen = ({ className, ...props }: AuthenticationMethodsScreenProps) => {
  const themeConfig = useTheme();

  return (
    <View safeArea {...props} className="flex-1">
      <View className="px-6 pt-24 pb-6">
        <Image
          className="w-20 h-20 self-center mb-6"
          source={require("@/assets/images/right-arrow-256x256.png")}
        />
        <Text className="self-center mb-1 font-bold" variant="titleLarge">
          Welcome to NextSolution
        </Text>
        <Text className="self-center text-on-surface-variant" variant="bodyMedium">
          Kickstart your mobile app with our template
        </Text>
      </View>
      <View className="px-6 flex-1">
        <Button className="mb-3" mode="contained" onPress={() => {}}>
          Sign in with email or phone
        </Button>
        <TouchableRipple
          className="rounded-full p-3"
          onPress={() => {
            router.push("/sign-up");
          }}
        >
          <Text className="text-center">
            Don't have an account? <Text className="text-primary font-bold">Create one</Text>
          </Text>
        </TouchableRipple>
        <Divider className="pt-1" alignment="center">
          <Text className="w-12 text-center" variant="bodyMedium">
            or
          </Text>
        </Divider>
        <Button
          className="mb-6"
          mode="elevated"
          icon={() => <GoogleColorIcon width={24} height={24} />}
          onPress={() => {}}
          buttonColor={themeConfig.colors.surfaceVariant}
          textColor={themeConfig.colors.onBackground}
          style={{
            shadowColor: "transparent"
          }}
          contentStyle={{
            justifyContent: "space-between"
          }}
          labelStyle={{
            justifyContent: "center",
            flex: 1
          }}
        >
          Continue with Google
        </Button>
        <Button
          mode="elevated"
          icon={() => <FacebookColorIcon width={24} height={24} />}
          buttonColor={themeConfig.colors.surfaceVariant}
          textColor={themeConfig.colors.onBackground}
          onPress={() => {}}
          style={{
            shadowColor: "transparent"
          }}
          contentStyle={{
            justifyContent: "space-between"
          }}
          labelStyle={{
            justifyContent: "center",
            flex: 1
          }}
        >
          Continue with Facebook
        </Button>
      </View>
      <View className="px-6 pt-4 pb-6">
        <Text className="text-center text-on-surface-variant" variant="bodySmall">
          By signing in, you agree to our{" "}
          <Text
            className="text-primary font-bold underline"
            pressedClassName="text-on-primary-container"
            variant="bodySmall"
          >
            Terms of Service
          </Text>{" "}
          and{" "}
          <Text
            className="text-primary font-bold underline"
            pressedClassName="text-on-primary-container"
            variant="bodySmall"
          >
            Privacy Policy.
          </Text>
        </Text>
      </View>
    </View>
  );
};

export type EnterPersonalDetailsScreenProps = ComponentProps<typeof View> & {};

const createSignUpScreen = (step: "enter-personal-details" | "enter-credentials") => {
  return ({ className, ...props }: EnterPersonalDetailsScreenProps) => {
    const snackbar = useSnackbar();

    const form = useFormContext<CreateAccountForm>();
    const [formSubmitting, setFormSubmitting] = useState(false);
    const formErrors = useMemoizedValue(form.formState.errors, formSubmitting);

    const handleSubmit = (e?: React.BaseSyntheticEvent) => {
      setFormSubmitting(true);
      return form.handleSubmit(async (inputs) => {
        const response = await identityService.createAccountAsync(inputs);
        setFormSubmitting(false);

        if (!response.success) {
          if (response instanceof ValidationProblem) {
            const errorFields = Object.entries<string[]>(response.errors || []);
            errorFields.forEach(([name, message]) => {
              form.setError(name as keyof CreateAccountForm, { message: message?.join("\n") });
            });

            if (errorFields.length <= 0) snackbar.show(response.message);
          } else {
            snackbar.show(response.message);
          }

          return;
        }
      })(e);
    };

    return (
      <View {...props} className="flex-1">
        <View className="px-6 pb-6">
          <Image
            className="w-16 h-16 self-center mb-6"
            source={require("@/assets/images/right-arrow-256x256.png")}
          />
          <Text className="self-center mb-1 font-bold" variant="titleLarge">
            {step === "enter-personal-details"
              ? "Enter your personal details"
              : "Enter your credentials"}
          </Text>
          <Text className="self-center text-on-surface-variant" variant="bodyMedium">
            {step == "enter-personal-details"
              ? "Let's get to know you better"
              : "We'll keep your account secure"}
          </Text>
        </View>
        {step == "enter-personal-details" && (
          <>
            <View className="px-6">
              <View className="pb-6">
                <TextInput label="First name" mode="outlined" autoFocus />
              </View>
              <View className="pb-6">
                <TextInput label="Last name" mode="outlined" />
              </View>
            </View>
            <View className="px-6 pt-4 pb-6">
              <Button
                mode="contained"
                onPress={() => {
                  router.push("/sign-up/enter-credentials");
                }}
              >
                Next
              </Button>
            </View>
          </>
        )}
        {step == "enter-credentials" && (
          <>
            <View className="px-6">
              <View className="pb-6">
                <TextInput label="Email" mode="outlined" autoFocus />
              </View>
              <View className="pb-6">
                <TextInput label="Password" mode="outlined" />
              </View>
            </View>
            <View className="px-6 pt-4 pb-6">
              <Button
                mode="contained"
                loading={formSubmitting}
                onPress={() => {
                  handleSubmit();
                }}
              >
                Create account
              </Button>
            </View>
          </>
        )}
      </View>
    );
  };
};

cssInterop(GoogleColorIcon, { className: "style" });

export { AuthScreen, createSignUpScreen };
