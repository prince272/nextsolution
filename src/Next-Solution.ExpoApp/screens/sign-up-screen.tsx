import React, { ComponentProps, useEffect, useRef, useState } from "react";
import { ScrollView } from "react-native";
import FacebookColorIcon from "@/assets/icons/facebook-round-color-icon.svg";
import GoogleColorIcon from "@/assets/icons/google-color-icon.svg";
import { Button, HelperText, Image, Text, TextInput, useSnackbar, View } from "@/components";
import { useMemoizedValue } from "@/hooks";
import { identityService } from "@/services";
import { useAuthentication } from "@/states";
import { useKeyboard } from "@react-native-community/hooks";
import { router } from "expo-router";
import { cssInterop } from "nativewind";
import { Controller, useFormContext } from "react-hook-form";
import { ValidationFailed } from "@/services/results";
import { CreateAccountForm } from "@/services/types";

export type SignUpScreenProps = ComponentProps<typeof View> & {};

const steps = ["enter-personal-details", "enter-credentials"] as const;

export type SignUpScreenSteps = (typeof steps)[number];

export type SignUpScreenStepFields = { [key in SignUpScreenSteps]: (keyof CreateAccountForm)[] };

const createSignUpScreen = (step: SignUpScreenSteps) => {
  return ({ className, ...props }: SignUpScreenProps) => {
    const snackbar = useSnackbar();
    const { keyboardShown } = useKeyboard();
    const scrollRef = useRef<ScrollView>(null);

    const form = useFormContext<CreateAccountForm>();
    const [formSubmitting, setFormSubmitting] = useState(false);
    const formErrors = useMemoizedValue(form.formState.errors, !formSubmitting);
    const formFields = useRef(
      (
        {
          "enter-personal-details": ["firstName", "lastName"],
          "enter-credentials": ["username", "password"]
        } as SignUpScreenStepFields
      )[step]
    ).current;
    const stepIndex = steps.indexOf(step);
    const nextStep = stepIndex !== -1 && stepIndex < steps.length - 1 ? steps[stepIndex + 1] : null;

    const { setUser: setCurrentUser } = useAuthentication();

    const handleSignUp = async () => {
      setFormSubmitting(true);
      return form.handleSubmit(async (inputs) => {
        const response = await identityService.createAccountAsync({ ...inputs });
        setFormSubmitting(false);

        if (!response.success) {
          if (response instanceof ValidationFailed) {
            const errorFields = Object.entries(response.errors || {}).filter((errorField) =>
              formFields.includes(errorField[0] as keyof CreateAccountForm)
            );

            errorFields.forEach(([name, message]) => {
              form.setError(name as keyof CreateAccountForm, { message: message?.join("\n") });
            });

            if (errorFields.length > 0) return;

            if (nextStep) {
              router.push(`/sign-up/${nextStep}`);
              return;
            }

            snackbar.show(response.message);
            return;
          } else {
            snackbar.show(response.message);
            return;
          }
        }

        setCurrentUser(response.data);
        router.replace("/");
      })();
    };

    useEffect(() => {
      return () => {
        formFields.forEach((field) => {
          form.resetField(field);
        });
      };
    }, []);

    useEffect(() => {
      if (keyboardShown) {
        scrollRef.current?.scrollTo({ y: 67 });
      }
    }, [keyboardShown]);

    const HeaderView = (
      <View className="px-6 pb-6">
        <Image
          className="w-16 h-16 self-center mb-3"
          source={require("@/assets/images/icon.png")}
        />
        <Text className="self-center mb-1 font-bold" variant="titleLarge">
          {
            {
              "enter-personal-details": "Create Your New Account",
              "enter-credentials": "Set up Your Credentials"
            }[step]
          }
        </Text>
        <Text className="self-center text-gray-600" variant="bodyMedium">
          {
            {
              "enter-personal-details": "Enter your personal details to continue",
              "enter-credentials": "Enter your credentials to create your account"
            }[step]
          }
        </Text>
      </View>
    );

    return (
      <View {...props} className="flex-1">
        {step == "enter-personal-details" && (
          <>
            <ScrollView ref={scrollRef} className="flex-grow-0">
              {HeaderView}
              <View className="px-6">
                <View>
                  <Controller
                    control={form.control}
                    render={({ field: { onChange, onBlur, value } }) => (
                      <>
                        <TextInput
                          mode="outlined"
                          autoFocus
                          label="First name"
                          onBlur={onBlur}
                          onChangeText={onChange}
                          value={value}
                        />
                        <HelperText type="error" padding="none">
                          {formErrors.firstName?.message ?? " "}
                        </HelperText>
                      </>
                    )}
                    name="firstName"
                  />
                </View>
                <View>
                  <Controller
                    control={form.control}
                    render={({ field: { onChange, onBlur, value } }) => (
                      <>
                        <TextInput
                          mode="outlined"
                          label="Last name (optional)"
                          onBlur={onBlur}
                          onChangeText={onChange}
                          value={value}
                        />
                        <HelperText type="error">{formErrors.lastName?.message ?? " "}</HelperText>
                      </>
                    )}
                    name="lastName"
                  />
                </View>
              </View>
            </ScrollView>
            <View className="px-6 pt-3 pb-6">
              <Button
                mode="contained"
                loading={formSubmitting}
                onPress={() => {
                  handleSignUp();
                }}
              >
                {!formSubmitting ? "Continue" : " "}
              </Button>
            </View>
          </>
        )}
        {step == "enter-credentials" && (
          <>
            <ScrollView ref={scrollRef} className="flex-grow-0">
              {HeaderView}
              <View className="px-6">
                <View>
                  <Controller
                    control={form.control}
                    render={({ field: { onChange, onBlur, value } }) => (
                      <>
                        <TextInput
                          mode="outlined"
                          autoFocus
                          label="Email or phone number"
                          onBlur={onBlur}
                          onChangeText={onChange}
                          value={value}
                        />
                        <HelperText type="error">{formErrors.username?.message ?? " "}</HelperText>
                      </>
                    )}
                    name="username"
                  />
                </View>
                <View>
                  <Controller
                    control={form.control}
                    render={({ field: { onChange, onBlur, value } }) => (
                      <>
                        <TextInput
                          mode="outlined"
                          secureTextEntry
                          autoCorrect={false}
                          label="Password"
                          onBlur={onBlur}
                          onChangeText={onChange}
                          value={value}
                        />
                        <HelperText type="error">{formErrors.password?.message ?? " "}</HelperText>
                      </>
                    )}
                    name="password"
                  />
                </View>
              </View>
            </ScrollView>
            <View className="px-6 pt-3 pb-3">
              <Button
                mode="contained"
                loading={formSubmitting}
                onPress={() => {
                  handleSignUp();
                }}
              >
                {!formSubmitting ? "Create account" : " "}
              </Button>
            </View>
          </>
        )}
      </View>
    );
  };
};

cssInterop(GoogleColorIcon, { className: "style" });
cssInterop(FacebookColorIcon, { className: "style" });

const EnterPersonalDetailsScreen = createSignUpScreen("enter-personal-details");
const EnterCredentialsScreen = createSignUpScreen("enter-credentials");

export { EnterPersonalDetailsScreen, EnterCredentialsScreen };
