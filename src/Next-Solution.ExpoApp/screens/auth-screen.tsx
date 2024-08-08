import React, { ComponentProps, useEffect, useRef, useState } from "react";
import { ScrollView } from "react-native";
import FacebookColorIcon from "@/assets/icons/facebook-round-color-icon.svg";
import GoogleColorIcon from "@/assets/icons/google-color-icon.svg";
import {
  Button,
  Divider,
  HelperText,
  Image,
  Text,
  TextInput,
  useSnackbar,
  View
} from "@/components";
import { useMemoizedValue } from "@/hooks";
import { identityService } from "@/services";
import { useKeyboard } from "@react-native-community/hooks";
import { router } from "expo-router";
import { cssInterop } from "nativewind";
import { Controller, useFormContext } from "react-hook-form";
import { TouchableRipple, useTheme } from "react-native-paper";
import { ValidationProblem } from "@/services/results";
import { CreateAccountForm } from "@/services/types";

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
          Welcome to Next Solution
        </Text>
        <Text className="self-center text-on-surface-variant" variant="bodyMedium">
          Kickstart your mobile app with our template
        </Text>
      </View>
      <View className="px-6 flex-1">
        <Button className="mb-3" mode="contained" onPress={() => {}}>
          Sign in with email or phone
        </Button>
        <View className="rounded-full">
          <TouchableRipple
            className="p-3 rounded-full"
            borderless
            onPress={() => {
              router.push("/sign-up");
            }}
          >
            <Text className="text-center">
              Don't have an account? <Text className="text-primary font-bold">Create one</Text>
            </Text>
          </TouchableRipple>
        </View>
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
    const { keyboardShown } = useKeyboard();
    const scrollRef = useRef<ScrollView>(null);

    const form = useFormContext<CreateAccountForm>();
    const [formSubmitting, setFormSubmitting] = useState(false);
    const formErrors = useMemoizedValue(form.formState.errors, !formSubmitting);

    const handleSubmit = async (validateOnly?: boolean) => {
      setFormSubmitting(true);
      return form.handleSubmit(async (inputs) => {
        const response = await identityService.createAccountAsync({ ...inputs, validateOnly });
        setFormSubmitting(false);

        if (!response.success) {
          if (response instanceof ValidationProblem) {
            const errorFields = Object.entries<string[]>(response.errors || []).filter(
              (errorField) =>
                ({
                  "enter-personal-details": ["firstName", "lastName"],
                  "enter-credentials": ["username", "password"]
                })[step].includes(errorField[0])
            );

            errorFields.forEach(([name, message]) => {
              form.setError(name as keyof CreateAccountForm, { message: message?.join("\n") });
            });

            if (errorFields.length > 0) return;

            if (step == "enter-personal-details") {
              router.push("/sign-up/enter-credentials");
              return;
            }
          } else {
            snackbar.show(response.message);
            return;
          }
        }
      })();
    };

    useEffect(() => {
      return () => {
        if (step === "enter-personal-details") {
          form.resetField("firstName");
          form.resetField("lastName");
        } else if (step === "enter-credentials") {
          form.resetField("username");
          form.resetField("password");
        }
      };
    }, []);

    useEffect(() => {
      if (keyboardShown) {
        scrollRef.current?.scrollTo({ y: 67 });
      }
    }, [keyboardShown]);

    const HeaderView = (
      <View className="px-6 pb-3">
        <Image
          className="w-16 h-16 self-center mb-3"
          source={require("@/assets/images/right-arrow-256x256.png")}
        />
        <Text className="self-center mb-1 font-bold" variant="titleLarge">
          {step === "enter-personal-details"
            ? "Enter your personal details"
            : "Enter your new credentials"}
        </Text>
        <Text className="self-center text-on-surface-variant" variant="bodyMedium">
          {step == "enter-personal-details"
            ? "Let's get to know you better"
            : "We'll keep your account secure"}
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
                        <HelperText type="error" padding="none">{formErrors.firstName?.message ?? " "}</HelperText>
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
                          label="Last name"
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
                  handleSubmit(true);
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
                          label="Email or phone"
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
            <View className="px-6 pt-3 pb-6">
              <Button
                mode="contained"
                loading={formSubmitting}
                onPress={() => {
                  handleSubmit();
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

export { AuthScreen, createSignUpScreen };
