import React, { ComponentProps, useCallback, useEffect, useState } from "react";
import FacebookColorIcon from "@/assets/icons/facebook-round-color-icon.svg";
import GoogleColorIcon from "@/assets/icons/google-color-icon.svg";
import { Button, Divider, Image, Text, useSnackbar, View } from "@/components";
import { identityService } from "@/services";
import { useAuthentication } from "@/states";
import { sleep } from "@/utils";
import * as Linking from "expo-linking";
import { router } from "expo-router";
import * as WebBrowser from "expo-web-browser";
import { TouchableRipple, useTheme } from "react-native-paper";
import { SignInWithProvider } from "@/services/types";

export type WelcomeScreenProps = ComponentProps<typeof View> & {};

const WelcomeScreen = ({ className, ...props }: WelcomeScreenProps) => {
  const [signingInWith, setSigningInWith] = useState<SignInWithProvider | null>(null);
  const themeConfig = useTheme();
  const snackbar = useSnackbar();

  const { setUser: setCurrentUser } = useAuthentication();
  const linkingUrl = Linking.useURL();

  const handleSignInWith = useCallback(async (provider: SignInWithProvider) => {
    try {
      setSigningInWith(provider);
      await sleep(2000);
      const callbackUrl = Linking.createURL("/");
      const redirectUrl = identityService.signInWithRedirect(provider, callbackUrl);
      await WebBrowser.openAuthSessionAsync(redirectUrl);
    } finally {
      setSigningInWith(null);
    }
  }, []);

  const handleSignInWithCallback = useCallback(
    async (provider: SignInWithProvider, token: string) => {
      const response = await identityService.SignInWithAsync(provider, token);

      if (!response.success) {
        console.log("Sign in failed:", response);
        snackbar.show(response.message);
        return;
      }

      console.log("User signed in:", response.data);
      setCurrentUser(response.data);
      router.replace("/");
    },
    []
  );

  const openSignUp = useCallback(() => router.push("/sign-up"), []);

  const openSignIn = useCallback(() => router.push("/sign-in"), []);

  useEffect(() => {
    if (linkingUrl) {
      const { hostname, path, queryParams } = Linking.parse(linkingUrl);
      const { token, provider } = queryParams || {};
      if (token && provider)
        handleSignInWithCallback(provider as SignInWithProvider, token as string);
    }
  }, [linkingUrl]);

  return (
    <View safeArea {...props} className="flex-1">
      <View className="px-6 pt-24 pb-6">
        <Image
          className="w-20 h-20 self-center mb-6"
          source={require("@/assets/images/icon.png")}
        />
        <Text className="self-center mb-1 font-bold" variant="titleLarge">
          Welcome to Next Solution
        </Text>
        <Text className="self-center text-on-surface-variant" variant="bodyMedium">
          Kickstart your mobile app with this template
        </Text>
      </View>
      <View className="px-6 flex-1">
        <Button className="mb-3" mode="contained" onPress={openSignIn}>
          Sign in with email or phone
        </Button>
        <TouchableRipple className="p-3 px-6 rounded-full" borderless onPress={openSignUp}>
          <Text className="text-center">
            Don't have an account? <Text className="text-primary font-bold">Create account</Text>
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
          onPress={() => {
            handleSignInWith("Google");
          }}
          buttonColor={themeConfig.colors.surfaceVariant}
          textColor={themeConfig.colors.onBackground}
          loading={signingInWith === "Google"}
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
          loading={signingInWith === "Facebook"}
          onPress={() => {
            handleSignInWith("Facebook");
          }}
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

export { WelcomeScreen };
