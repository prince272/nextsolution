"use client";

import { FC, useRef, useState } from "react";
import NextLink from "next/link";
import { usePathname, useSearchParams } from "next/navigation";
import { ChevronLeftIcon, ChevronRightIcon, GoogleIcon, PersonIcon } from "@/assets/icons";
import { Button } from "@nextui-org/button";
import { Input } from "@nextui-org/input";
import { Link } from "@nextui-org/link";
import { Modal, ModalBody, ModalContent, ModalFooter, ModalHeader } from "@nextui-org/modal";
import { Spinner } from "@nextui-org/spinner";
import { clone } from "lodash";
import queryString from "query-string";
import { Controller, SubmitHandler, useForm } from "react-hook-form";
import toast from "react-hot-toast";
import { v4 as uuidv4 } from "uuid";

import { useApi } from "@/lib/api/client";
import { getApiErrorMessage, isApiError } from "@/lib/api/utils";
import { useConditionalState } from "@/lib/hooks";
import { cn } from "@/lib/utils";
import { PasswordInput } from "@/components/ui/password-input";
import { PhoneInput } from "@/components/ui/phone-input";

export type SignInMethods = "credentials" | "google";

export interface SignInProps {
  opened: boolean;
  onClose: (force?: boolean) => void;
}

export interface SignInInputs {
  username: string;
  password: string;
}

export const SignInModal: FC<SignInProps> = ({ opened, onClose }) => {
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [method, setMethod] = useState<SignInMethods | null>(searchParams.get("method") as any);
  const api = useApi();
  const form = useForm<SignInInputs>({
    defaultValues: {
      username: "",
      password: ""
    }
  });
  const formErrors = useConditionalState(clone(form.formState.errors), !form.formState.isSubmitting);
  const componentId = useRef(uuidv4()).current;

  const [state, setState] = useState<{ action: "idle" | "loading" | "submitting"; error?: any }>({ action: "idle", error: null });

  const onSignIn: SubmitHandler<SignInInputs> = async (inputs) => {
    try {
      setState({ action: "submitting" });
      await api.signIn(inputs);
      onClose(false);
    } catch (error) {
      setState({ action: "idle", error });

      if (isApiError(error)) {
        if (error.response) {
          const fields = Object.entries<string[]>(error.response.data.errors || []);
          fields.forEach(([name, message]) => {
            form.setError(name as any, { message: message?.join("\n") });
          });
        }
      }

      toast.error(getApiErrorMessage(error), { id: componentId });
    }
  };

  const onSignInWith = async (provider: Exclude<SignInMethods, "credentials">) => {
    try {
      setMethod(provider);
      await api.signInWith(provider);
      onClose(false);
    } catch (error) {
      setMethod(null);
      toast.error(getApiErrorMessage(error), { id: componentId });
    }
  };

  return (
    <>
      <Modal isOpen={opened} onClose={onClose}>
        <ModalContent>
          <ModalHeader className="flex flex-col gap-1">Sign in to your account</ModalHeader>
          <ModalBody>
            <form onSubmit={form.handleSubmit(onSignIn)}>
              <div className={cn("grid gap-y-5 pb-2.5", method != "credentials" && "hidden")}>
                <Controller
                  control={form.control}
                  name="username"
                  render={({ field: { onChange, onBlur, value, ref } }) => (
                    <PhoneInput
                      ref={ref}
                      onChange={(e) => onChange(e.target.value)}
                      onBlur={onBlur}
                      value={value}
                      isInvalid={!!formErrors.username}
                      errorMessage={formErrors.username?.message}
                      autoComplete="off"
                      label="Email or phone number"
                      countryVisibility="auto"
                    />
                  )}
                />
                <div>
                  <Controller
                    control={form.control}
                    name="password"
                    render={({ field: { onChange, onBlur, value, ref } }) => (
                      <PasswordInput
                        ref={ref}
                        onChange={(e) => onChange(e.target.value)}
                        onBlur={onBlur}
                        value={value}
                        isInvalid={!!formErrors.password}
                        errorMessage={formErrors.password?.message}
                        autoComplete="off"
                        label="Password"
                      />
                    )}
                  />
                  <div className="flex items-center justify-end pt-2 text-end text-sm">
                    <Link as={NextLink} className="text-sm" href={queryString.stringifyUrl({ url: pathname, query: { dialogId: "reset-password" } })}>
                      Forgot password?
                    </Link>
                  </div>
                </div>
                <Button color="primary" type="submit" isLoading={state.action == "submitting"}>
                  Sign in
                </Button>
                <div className="flex items-center justify-center text-center text-sm">
                  <Link as="button" className="text-sm" type="button" onPress={() => setMethod(null)}>
                    <ChevronLeftIcon className="h-4 w-4" /> Go back
                  </Link>
                </div>
              </div>
            </form>
            <div className={cn("grid gap-y-5", method == "credentials" && "hidden")}>
              <Button
                startContent={<PersonIcon className="h-6 w-6" />}
                color="primary"
                onPress={() => {
                  setMethod("credentials");
                  form.reset();
                }}
              >
                Use email or phone
              </Button>
              <Button
                startContent={method != "google" && <GoogleIcon className="h-6 w-6" />}
                isLoading={method == "google"}
                spinner={<Spinner color="current" size="sm" />}
                onPress={() => onSignInWith("google")}
              >
                Continue with Google
              </Button>
            </div>
          </ModalBody>
          <ModalFooter className="flex items-center justify-center text-center text-sm">
            Don&apos;t have an account?{" "}
            <Link as={NextLink} className="text-sm" href={queryString.stringifyUrl({ url: pathname, query: { dialogId: "sign-up", method: method ?? undefined } })}>
              Sign up <ChevronRightIcon className="h-4 w-4" />
            </Link>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </>
  );
};
