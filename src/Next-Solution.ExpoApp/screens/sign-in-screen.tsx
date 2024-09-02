import React, { ComponentProps, useCallback, useEffect, useRef, useState } from "react";
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
  TouchableRipple,
  useSnackbar,
  View
} from "@/components";
import { useMemoizedValue } from "@/hooks";
import { identityService } from "@/services";
import { useAuthentication } from "@/states";
import { useKeyboard } from "@react-native-community/hooks";
import { router } from "expo-router";
import { cssInterop } from "nativewind";
import { Controller, useFormContext } from "react-hook-form";
import { ValidationFailed } from "@/services/results";
import { SignInForm } from "@/services/types";

export type SignInScreenProps = ComponentProps<typeof View> & {};

const SignInScreen = ({ className, ...props }: SignInScreenProps) => {
  const snackbar = useSnackbar();
  const { keyboardShown } = useKeyboard();
  const scrollRef = useRef<ScrollView>(null);

  const form = useFormContext<SignInForm>();
  const [formSubmitting, setFormSubmitting] = useState(false);
  const formErrors = useMemoizedValue(form.formState.errors, !formSubmitting);

  const { setUser: setCurrentUser } = useAuthentication();

  const handleSignIn = useCallback(async () => {
    setFormSubmitting(true);
    return form.handleSubmit(async (inputs) => {
      const response = await identityService.signInAsync({ ...inputs });
      setFormSubmitting(false);

      if (!response.success) {
        if (response instanceof ValidationFailed) {
          const errorFields = Object.entries<string[]>(response.errors || []);

          errorFields.forEach(([name, message]) => {
            form.setError(name as keyof SignInForm, { message: message?.join("\n") });
          });

          if (errorFields.length > 0) return;

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
  }, []);

  const openResetPassword = useCallback(() => {
    router.push("/reset-password");
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
        Sign into Your Account
      </Text>
      <Text className="self-center text-on-surface-variant" variant="bodyMedium">
        Enter your credentials to access your account
      </Text>
    </View>
  );

  return (
    <View {...props} className="flex-1">
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
              handleSignIn();
            }}
          >
            {!formSubmitting ? "Sign in" : " "}
          </Button>
        </View>
        <View>
          <TouchableRipple
            className="p-3 px-6 rounded-full self-center"
            borderless
            onPress={openResetPassword}
          >
            <Text className="text-center">
              Forgot your password? <Text className="text-primary font-bold">Reset password</Text>
            </Text>
          </TouchableRipple>
        </View>
      </>
    </View>
  );
};

cssInterop(GoogleColorIcon, { className: "style" });
cssInterop(FacebookColorIcon, { className: "style" });

export { SignInScreen };
