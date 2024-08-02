import { ComponentProps, useState } from "react";
import {
  Keyboard,
  KeyboardAvoidingView,
  ScrollView,
  TouchableWithoutFeedback,
  View
} from "react-native";
import FacebookRoundColorIcon from "@/assets/icons/facebook-round-color-icon.svg";
import GoogleColorIcon from "@/assets/icons/google-color-icon.svg";
import { identityService } from "@/services";
import { cn } from "@/utils";
import { Image } from "expo-image";
import { Link } from "expo-router";
import { cssInterop } from "nativewind";
import { Controller, useForm } from "react-hook-form";
import { KeyboardAwareScrollView } from "react-native-keyboard-aware-scroll-view";
import {
  Button,
  customText,
  HelperText,
  Icon,
  TextInput,
  TouchableRipple,
  useTheme
} from "react-native-paper";
import { ValidationProblem } from "@/services/results";
import { CreateAccountForm } from "@/services/types";
import { useMemoizedValue } from "@/hooks/use-memoized-value";
import { useSnackbar } from "@/components/providers/snackbar";

const Text = customText();

export interface SignUpScreenProps extends ComponentProps<typeof View> {
  formOnly?: boolean;
}

export const SignUpScreen = ({ className, formOnly, ...props }: SignUpScreenProps) => {
  const form = useForm<CreateAccountForm>();
  const [formSubmitting, setFormSubmitting] = useState(false);
  const formErrors = useMemoizedValue(form.formState.errors, formSubmitting);

  const snackbar = useSnackbar();

  const themeConfig = useTheme();

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
    <TouchableWithoutFeedback onPress={Keyboard.dismiss}>
      <View {...props} className={cn("flex-1", className)}>
        {/* Main Content */}
        <View className={cn("flex-1")}>
          {!formOnly && (
            <View>
              <View className="pb-9 px-6">
                <Image
                  className="w-24 h-24 self-center mb-9"
                  source={require("@/assets/images/right-arrow-512x512.png")}
                />
                <Text variant="titleLarge" className="text-center font-bold">
                  Welcome to Next Solution
                </Text>
                <Text className="text-center text-on-surface-variant">
                  Kickstart your app with our codebase template!
                </Text>
              </View>
              <View className="px-6">
                <Link href="/sign-up-form" asChild>
                  <Button
                    icon={({ color }) => <Icon source="account" size={24} color={color} />}
                    className="rounded-full mb-4"
                    mode="contained"
                  >
                    Sign up with email or phone
                  </Button>
                </Link>
                <Link href="/sign-in-form" asChild>
                  <TouchableRipple onPress={() => {}}>
                    <Text className="text-center py-1">
                      Already have an account?{" "}
                      <Text className="text-primary font-bold">Sign in</Text>
                    </Text>
                  </TouchableRipple>
                </Link>
                <View className="flex-row items-center my-6">
                  <View className="flex-1 h-px bg-outline-variant" />
                  <View>
                    <Text className="w-12 text-center">or</Text>
                  </View>
                  <View className="flex-1 h-px bg-outline-variant" />
                </View>
                {/* Google sign provider */}
                <Button
                  icon={() => <GoogleColorIcon width={24} height={24} />}
                  className="rounded-full mb-6"
                  buttonColor={themeConfig.colors.surfaceVariant}
                  textColor={themeConfig.colors.onBackground}
                  mode="elevated"
                  onPress={() => {}}
                >
                  Continue with Google
                </Button>
                {/* Facebook sign provider */}
                <Button
                  icon={() => <FacebookRoundColorIcon width={24} height={24} />}
                  className="rounded-full mb-6"
                  buttonColor={themeConfig.colors.surfaceVariant}
                  textColor={themeConfig.colors.onBackground}
                  mode="elevated"
                  onPress={() => {}}
                >
                  Continue with Facebook
                </Button>
              </View>
            </View>
          )}

          {formOnly && (
            <View>
              <View className="flex-row items-start pb-9 px-6 absolute w-full">
                <Image
                  className="w-12 h-12 mr-4"
                  source={require("@/assets/images/right-arrow-512x512.png")}
                />
                <View className="flex-1">
                  <Text variant="titleLarge" className="font-bold">
                    Create a new account
                  </Text>
                  <Text className="text-on-surface-variant" variant="bodySmall">
                    Enter your details to sign up.
                  </Text>
                </View>
              </View>
              <KeyboardAwareScrollView className="px-6 mt-20" extraHeight={500}>
                <View className="flex-row pb-4">
                  <View className="flex-1 pr-2">
                    <Text variant="labelLarge" className="mb-2">
                      First name
                    </Text>
                    <Controller
                      control={form.control}
                      render={({ field: { onChange, onBlur, value } }) => (
                        <>
                          <TextInput
                            dense
                            mode="flat"
                            onBlur={onBlur}
                            onChangeText={onChange}
                            value={value}
                          />
                          {formErrors.firstName && (
                            <HelperText type="error">{formErrors.firstName.message}</HelperText>
                          )}
                        </>
                      )}
                      name="firstName"
                    />
                  </View>
                  <View className="flex-1 pl-2">
                    <Text variant="labelLarge" className="mb-2">
                      Last Name
                    </Text>
                    <Controller
                      control={form.control}
                      render={({ field: { onChange, onBlur, value } }) => (
                        <>
                          <TextInput
                            dense
                            mode="flat"
                            onBlur={onBlur}
                            onChangeText={onChange}
                            value={value}
                          />
                          {formErrors.lastName && (
                            <HelperText type="error">{formErrors.lastName.message}</HelperText>
                          )}
                        </>
                      )}
                      name="lastName"
                    />
                  </View>
                </View>
                <View className="pb-4">
                  <Text variant="labelLarge" className="mb-2">
                    Email or phone number
                  </Text>
                  <Controller
                    control={form.control}
                    render={({ field: { onChange, onBlur, value } }) => (
                      <>
                        <TextInput
                          dense
                          mode="flat"
                          onBlur={onBlur}
                          onChangeText={onChange}
                          value={value}
                        />
                        {formErrors.username && (
                          <HelperText type="error">{formErrors.username.message}</HelperText>
                        )}
                      </>
                    )}
                    name="username"
                  />
                </View>
                <View className="pb-4">
                  <Text variant="labelLarge" className="mb-2">
                    Password
                  </Text>
                  <Controller
                    control={form.control}
                    render={({ field: { onChange, onBlur, value } }) => (
                      <>
                        <TextInput
                          dense
                          mode="flat"
                          secureTextEntry
                          onBlur={onBlur}
                          onChangeText={onChange}
                          value={value}
                        />
                        {formErrors.password && (
                          <HelperText type="error">{formErrors.password.message}</HelperText>
                        )}
                      </>
                    )}
                    name="password"
                  />
                </View>
              </KeyboardAwareScrollView>
            </View>
          )}
        </View>

        {/* Footer */}
        {formOnly && (
          <View className="px-6 pt-4 pb-3 bg-surface">
            <Button
              onPress={() => {
                Keyboard.dismiss();
                handleSubmit();
              }}
              className="rounded-full mb-4"
              mode="contained"
              loading={formSubmitting}
              disabled={formSubmitting}
            >
              Sign up
            </Button>
            <View>
              <Link href="/sign-in-form" asChild>
                <TouchableRipple>
                  <Text className="text-center py-1">
                    Already have an account? <Text className="text-primary font-bold">Sign in</Text>
                  </Text>
                </TouchableRipple>
              </Link>
            </View>
          </View>
        )}

        {/* Footer */}
        {!formOnly && (
          <View className="pb-6 px-2">
            <Text className="text-center">
              By signing up you accept our{" "}
              <Text className="text-primary underline font-bold">Terms of Service</Text> and{" "}
              <Text className="text-primary underline font-bold">Privacy Policy</Text>
            </Text>
          </View>
        )}
      </View>
    </TouchableWithoutFeedback>
  );
};

// Resolves third-party className to styles at runtime.
cssInterop(ScrollView, { className: "style" });
cssInterop(Image, { className: "style" });
cssInterop(Text, { className: "style" });
cssInterop(Button, { className: "style" });
cssInterop(TextInput, { className: "style" });
