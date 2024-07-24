import { ComponentProps } from "react";
import { Keyboard, ScrollView, TouchableWithoutFeedback, View } from "react-native";
import FacebookRoundColorIcon from "@/assets/icons/facebook-round-color-icon.svg";
import GoogleColorIcon from "@/assets/icons/google-color-icon.svg";
import { identityService } from "@/services";
import { cn } from "@/utils";
import { Image } from "expo-image";
import { Link } from "expo-router";
import { cssInterop } from "nativewind";
import { Controller, useForm } from "react-hook-form";
import { KeyboardAwareScrollView } from "react-native-keyboard-aware-scroll-view";
import { Button, customText, HelperText, Icon, TextInput, TouchableRipple, useTheme } from "react-native-paper";
import { ValidationProblem } from "@/services/results";
import { SignInForm } from "@/services/types";
import { useConditionalState } from "@/hooks/use-conditional-state";
import { clone } from "lodash";

const Text = customText();

export interface SignInScreenProps extends ComponentProps<typeof View> {
  formOnly?: boolean;
}

export const SignInScreen = ({ className, formOnly, ...props }: SignInScreenProps) => {
  const form = useForm<SignInForm>();
  const formErrors = useConditionalState(clone(form.formState.errors), !form.formState.isSubmitting);
  
  const themeConfig = useTheme();

  const handleSubmit = form.handleSubmit(async (inputs) => {
    const response = await identityService.signIn(inputs);
    if (!response.success) {
      if (response instanceof ValidationProblem) {
        const errorFields = Object.entries<string[]>(response.errors || []);
        errorFields.forEach(([name, message]) => {
          form.setError(name as keyof SignInForm, { message: message?.join("\n") });
        });
      }
      return;
    }
  });

  return (
    <TouchableWithoutFeedback onPress={Keyboard.dismiss}>
      <View {...props} className={cn("flex-1", className)}>
        {/* Main Content */}
        <View className={cn(!formOnly && "flex-1")}>
          {/* Header */}
          {!formOnly && (
            <View className="mb-9 mx-6">
              <Image className="w-24 h-24 self-center mb-9" source={require("@/assets/images/right-arrow-512x512.png")} />
              <Text variant="titleLarge" className="text-center font-bold">
                Welcome to Next Solution
              </Text>
              <Text className="text-center text-on-surface-variant">Kickstart your app with our codebase template!</Text>
            </View>
          )}

          {formOnly && (
            <View className="flex-row items-start mb-9 px-6">
              <Image className="w-12 h-12 mr-4" source={require("@/assets/images/right-arrow-512x512.png")} />
              <View className="flex-1">
                <Text variant="titleLarge" className="font-bold">
                  Sign into your account
                </Text>
                <Text className="text-on-surface-variant" variant="bodySmall">
                  Enter your credentials to access your account
                </Text>
              </View>
            </View>
          )}

          {/* Form */}

          {formOnly && (
            <KeyboardAwareScrollView>
              <View className="mx-6">
                <View className="mb-4">
                  <Text variant="labelLarge" className="mb-2">
                    Email or phone number
                  </Text>
                  <Controller
                    control={form.control}
                    render={({ field: { onChange, onBlur, value } }) => (
                      <>
                        <TextInput dense mode="outlined" onBlur={onBlur} onChangeText={onChange} value={value} />
                        {formErrors.username && <HelperText type="error">{formErrors.username.message}</HelperText>}
                      </>
                    )}
                    name="username"
                  />
                </View>
                <View className="mb-4">
                  <Text variant="labelLarge" className="mb-2">
                    Password
                  </Text>
                  <Controller
                    control={form.control}
                    render={({ field: { onChange, onBlur, value } }) => <TextInput dense mode="outlined" secureTextEntry onBlur={onBlur} onChangeText={onChange} value={value} />}
                    name="password"
                  />
                </View>
              </View>
            </KeyboardAwareScrollView>
          )}

          {formOnly && (
            <View className="mx-6 my-6">
              <Button
                onPress={() => {
                  handleSubmit();
                }}
                className="rounded-md mb-4"
                mode="contained"
              >
                Sign in
              </Button>
              <View>
                <TouchableRipple onPress={() => {}}>
                  <Text className="text-center py-1">
                    Forgot your password? <Text className="text-primary font-bold">Reset it here</Text>
                  </Text>
                </TouchableRipple>
              </View>
            </View>
          )}

          {!formOnly && (
            <View className="mx-6">
              <Link href="/sign-in-form" asChild>
                <Button icon={({ color }) => <Icon source="account" size={24} color={color} />} className="rounded-md mb-4" mode="contained">
                  Sign in with email or phone
                </Button>
              </Link>
              <TouchableRipple onPress={() => {}}>
                <Text className="text-center py-1">
                  Don't have an account? <Text className="text-primary font-bold">Create one</Text>
                </Text>
              </TouchableRipple>
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
                className="rounded-md mb-6"
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
                className="rounded-md mb-6"
                buttonColor={themeConfig.colors.surfaceVariant}
                textColor={themeConfig.colors.onBackground}
                mode="elevated"
                onPress={() => {}}
              >
                Continue with Facebook
              </Button>
            </View>
          )}
        </View>

        {/* Footer */}
        {!formOnly && (
          <View className="pb-6 px-2">
            <Text className="text-center">
              By signing in you accept our <Text className="text-primary underline font-bold">Terms of Service</Text> and{" "}
              <Text className="text-primary underline font-bold">Privacy Policy</Text>
            </Text>
          </View>
        )}
      </View>
    </TouchableWithoutFeedback>
  );
};

// Resolves third-party className to styles at runtime.
cssInterop(Image, { className: "style" });
cssInterop(Text, { className: "style" });
cssInterop(Button, { className: "style" });
cssInterop(TextInput, { className: "style" });
