import React, { ComponentProps, useEffect, useMemo, useRef, useState } from "react";
import { ScrollView } from "react-native";
import {
  Button,
  HelperText,
  Image,
  Text,
  TextInput,
  TouchableRipple,
  useSnackbar,
  View
} from "@/components";
import { useMemoizedValue, useTimer } from "@/hooks";
import { identityService } from "@/services";
import { useKeyboard } from "@react-native-community/hooks";
import { router } from "expo-router";
import { Controller, useFormContext } from "react-hook-form";
import { ValidationFailed } from "@/services/results";
import { ResetPasswordForm } from "@/services/types";

export type ResetPasswordScreenProps = ComponentProps<typeof View> & {};

const steps = ["enter-username", "enter-verification-code", "enter-new-password"] as const;

export type ResetPasswordScreenSteps = (typeof steps)[number];

export type ResetPasswordScreenStepFields = {
  [key in ResetPasswordScreenSteps]: (keyof ResetPasswordForm)[];
};

const createResetPasswordScreen = (step: ResetPasswordScreenSteps) => {
  return ({ className, ...props }: ResetPasswordScreenProps) => {
    const snackbar = useSnackbar();
    const { keyboardShown } = useKeyboard();
    const scrollRef = useRef<ScrollView>(null);

    const form = useFormContext<ResetPasswordForm>();
    const [formSubmitting, setFormSubmitting] = useState(false);
    const formErrors = useMemoizedValue(form.formState.errors, !formSubmitting);
    const formFields = useRef(
      (
        {
          "enter-username": ["username"],
          "enter-verification-code": ["code"],
          "enter-new-password": ["newPassword", "confirmPassword"]
        } as ResetPasswordScreenStepFields
      )[step]
    ).current;
    const formIsEmailAddress = useMemo(
      () => !/^[-+0-9() ]+$/.test(form.watch("username") ?? ""),
      [form.watch("username")]
    );

    const stepIndex = steps.indexOf(step);
    const nextStep = stepIndex != -1 && stepIndex < steps.length - 1 ? steps[stepIndex + 1] : null;

    const sendCodeTimer = useTimer({
      timerType: "DECREMENTAL",
      initialTime: 60,
      endTime: 0
    });

    const sendResetPasswordCode = async (resend: boolean = false) => {
      if (!resend) setFormSubmitting(true);

      return form.handleSubmit(async (inputs) => {
        const response = await identityService.sendResetPasswordCodeAsync(inputs);
        if (!resend) setFormSubmitting(false);

        if (!response.success) {
          if (response instanceof ValidationFailed) {
            const errorFields = Object.entries(response.errors || {});

            errorFields.forEach(([name, message]) => {
              form.setError(name as keyof ResetPasswordForm, { message: message?.join("\n") });
            });

            if (errorFields.length > 0) return;

            snackbar.show(response.message);
            return;
          } else {
            snackbar.show(response.message);
            return;
          }
        }

        snackbar.show(resend ? "Verification code resent!" : "Verification code sent!");
        sendCodeTimer.start();
        if (nextStep && !resend) router.push(`/reset-password/${nextStep}`);
      })();
    };

    const handleResetPassword = async () => {
      setFormSubmitting(true);

      return form.handleSubmit(async (inputs) => {
        const response = await identityService.resetPasswordAsync(inputs);
        setFormSubmitting(false);

        if (!response.success) {
          if (response instanceof ValidationFailed) {
            const errorFields = Object.entries(response.errors || {}).filter((errorField) =>
              formFields.includes(errorField[0] as keyof ResetPasswordForm)
            );

            errorFields.forEach(([name, message]) => {
              form.setError(name as keyof ResetPasswordForm, { message: message?.join("\n") });
            });

            if (errorFields.length > 0) return;

            if (nextStep) {
              router.push(`/reset-password/${nextStep}`);
              return;
            }

            snackbar.show(response.message);
            return;
          } else {
            snackbar.show(response.message);
            return;
          }
        }

        snackbar.show("Password reset successfully!");

        router.push("/sign-in");
      })();
    };

    useEffect(() => {
      if (step == "enter-verification-code") {
        sendCodeTimer.start();
      }
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
          {{
            "enter-username": "Reset Your Password",
            "enter-verification-code": "Verify Your Account",
            "enter-new-password": "Set up a New Password"
          }[step] || " "}
        </Text>
        <Text className="self-center text-gray-600" variant="bodyMedium">
          {{
            "enter-username": "We'll send a code to your email or phone number",
            "enter-verification-code": `Enter the code sent to your ${formIsEmailAddress ? "email address" : "phone number"}`,
            "enter-new-password": "Create a new password to complete the process"
          }[step] || " "}
        </Text>
      </View>
    );

    return (
      <View {...props} className="flex-1">
        {step == "enter-username" && (
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
              </View>
            </ScrollView>
            <View className="px-6 pt-3 pb-6">
              <Button
                mode="contained"
                loading={formSubmitting}
                onPress={() => {
                  sendResetPasswordCode();
                }}
              >
                {!formSubmitting ? "Send code" : " "}
              </Button>
            </View>
          </>
        )}
        {step == "enter-verification-code" && (
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
                          label="Verification code"
                          onBlur={onBlur}
                          onChangeText={onChange}
                          value={value}
                        />
                        <HelperText type="error">{formErrors.code?.message ?? " "}</HelperText>
                      </>
                    )}
                    name="code"
                  />
                </View>
              </View>
            </ScrollView>
            <View className="px-6 pt-3 pb-6">
              <Button
                mode="contained"
                loading={formSubmitting}
                onPress={() => {
                  handleResetPassword();
                }}
              >
                {!formSubmitting ? "Continue" : " "}
              </Button>
            </View>
            <View>
              <TouchableRipple
                className="p-3 px-6 rounded-full self-center"
                borderless
                onPress={() => {
                  sendResetPasswordCode(true);
                }}
                disabled={sendCodeTimer.isRunning}
              >
                <>
                  {sendCodeTimer.isRunning ? (
                    <Text className="text-center">
                      Didn't get the code? Try again in {sendCodeTimer.time}s
                    </Text>
                  ) : (
                    <Text className="text-center">
                      Didn't get the code?{" "}
                      <Text className="text-primary font-bold">Resend code</Text>
                    </Text>
                  )}
                </>
              </TouchableRipple>
            </View>
          </>
        )}
        {step == "enter-new-password" && (
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
                          secureTextEntry
                          autoCorrect={false}
                          label="New password"
                          onBlur={onBlur}
                          onChangeText={onChange}
                          value={value}
                        />
                        <HelperText type="error">
                          {formErrors.newPassword?.message ?? " "}
                        </HelperText>
                      </>
                    )}
                    name="newPassword"
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
                          label="Confirm password"
                          onBlur={onBlur}
                          onChangeText={onChange}
                          value={value}
                        />
                        <HelperText type="error">
                          {formErrors.confirmPassword?.message ?? " "}
                        </HelperText>
                      </>
                    )}
                    name="confirmPassword"
                  />
                </View>
              </View>
            </ScrollView>
            <View className="px-6 pt-3 pb-6">
              <Button
                mode="contained"
                loading={formSubmitting}
                onPress={() => {
                  handleResetPassword();
                }}
              >
                {!formSubmitting ? "Reset password" : " "}
              </Button>
            </View>
          </>
        )}
      </View>
    );
  };
};

const EnterUsernameScreen = createResetPasswordScreen("enter-username");
const EnterVerificationCodeScreen = createResetPasswordScreen("enter-verification-code");
const EnterNewPasswordScreen = createResetPasswordScreen("enter-new-password");

export { EnterUsernameScreen, EnterVerificationCodeScreen, EnterNewPasswordScreen };
