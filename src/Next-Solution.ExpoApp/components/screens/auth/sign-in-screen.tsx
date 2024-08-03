import { ComponentProps, useCallback, useEffect, useRef, useState } from "react";
import { Keyboard, ScrollView, TouchableWithoutFeedback, View } from "react-native";
import FacebookRoundColorIcon from "@/assets/icons/facebook-round-color-icon.svg";
import GoogleColorIcon from "@/assets/icons/google-color-icon.svg";
import { identityService } from "@/services";
import { useAppStore } from "@/stores";
import { cn, sleep } from "@/utils";
import { Image } from "expo-image";
import * as Linking from "expo-linking";
import { Link } from "expo-router";
import * as WebBrowser from "expo-web-browser";
import { delay } from "lodash";
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
import { SignInForm, SignInWithProvider } from "@/services/types";
import { useMemoizedValue } from "@/hooks/use-memoized-value";
import { useSnackbar } from "@/components/providers/snackbar";

const Text = customText();

export interface SignInScreenProps extends ComponentProps<typeof View> {
  formOnly?: boolean;
}

export const SignInScreen = ({ className, formOnly, ...props }: SignInScreenProps) => {
  const form = useForm<SignInForm>();
  const [formSubmitting, setFormSubmitting] = useState(false);
  const formErrors = useMemoizedValue(form.formState.errors, formSubmitting);

  const { clearUser, setUser, user: currentUser } = useAppStore((state) => state.authentication);
  const [signInWith, setSignInWith] = useState<SignInWithProvider | null>(null);
  const linkingUrl = Linking.useURL();

  const snackbar = useSnackbar();
  const themeConfig = useTheme();

  const handleSignIn = (e?: React.BaseSyntheticEvent) => {
    setFormSubmitting(true);
    return form.handleSubmit(async (inputs) => {
      const response = await identityService.signInAsync(inputs);
      setFormSubmitting(false);

      if (!response.success) {
        if (response instanceof ValidationProblem) {
          const errorFields = Object.entries<string[]>(response.errors || []);
          errorFields.forEach(([name, message]) => {
            form.setError(name as keyof SignInForm, { message: message?.join("\n") });
          });

          if (errorFields.length <= 0) snackbar.show(response.message);
        } else {
          snackbar.show(response.message);
        }

        return;
      }

      setUser(response.data);
    })(e);
  };

  const handleSignInWithRedirect = useCallback(async (provider: SignInWithProvider) => {
    setSignInWith(provider);
    await sleep(2000);
    const callbackUrl = Linking.createURL("/sign-in");
    const redirectUrl = identityService.signInWithRedirect(provider, callbackUrl);
    await WebBrowser.openAuthSessionAsync(redirectUrl);
    setSignInWith(null);
  }, []);

  const handleSignInWithCallback = useCallback(
    async (provider: SignInWithProvider, token: string) => {
      const response = await identityService.SignInWithAsync(provider, token);

      if (!response.success) {
        snackbar.show(response.message);
        return;
      }

      setUser(response.data);
    },
    []
  );

  useEffect(() => {
    if (linkingUrl) {
      const { hostname, path, queryParams } = Linking.parse(linkingUrl);
      const { token, provider } = queryParams || {};
      if (token && provider)
        handleSignInWithCallback(provider as SignInWithProvider, token as string);
    }
  }, [linkingUrl]);

  return (
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
              <Link href="/sign-in-form" asChild>
                <Button
                  icon={({ color }) => <Icon source="account" size={24} color={color} />}
                  className="rounded-full mb-4"
                  mode="contained"
                >
                  Sign in with email or phone
                </Button>
              </Link>
              <Link href="/sign-up-form" asChild>
                <TouchableRipple onPress={() => {}}>
                  <Text className="text-center py-1">
                    Don't have an account?{" "}
                    <Text className="text-primary font-bold">Create one</Text>
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
                onPress={() => handleSignInWithRedirect("Google")}
                loading={signInWith === "Google"}
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
                onPress={() => handleSignInWithRedirect("Facebook")}
                loading={signInWith === "Facebook"}
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
                  Sign into your account
                </Text>
                <Text className="text-on-surface-variant" variant="bodySmall">
                  Enter your credentials to sign in.
                </Text>
              </View>
            </View>
            <KeyboardAwareScrollView className="px-6 mt-20" extraHeight={500}>
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
              handleSignIn();
            }}
            className="rounded-full mb-4"
            mode="contained"
            loading={formSubmitting}
            disabled={formSubmitting}
          >
            Sign in
          </Button>
          <View>
            <Link href="/sign-up-form" asChild>
              <TouchableRipple>
                <Text className="text-center py-1">
                  Don't have an account? <Text className="text-primary font-bold">Create one</Text>
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
            By signing in you accept our{" "}
            <Text className="text-primary underline font-bold">Terms of Service</Text> and{" "}
            <Text className="text-primary underline font-bold">Privacy Policy</Text>
          </Text>
        </View>
      )}
    </View>
  );
};

// Resolves third-party className to styles at runtime.
cssInterop(ScrollView, { className: "style" });
cssInterop(Image, { className: "style" });
cssInterop(Text, { className: "style" });
cssInterop(Button, { className: "style" });
cssInterop(TextInput, { className: "style" });
