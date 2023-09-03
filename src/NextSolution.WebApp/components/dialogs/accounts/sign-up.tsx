"use client";

import React, { useCallback, useEffect, useId, useRef, useState } from "react";
import NextLink from "next/link";
import { usePathname, useSearchParams } from "next/navigation";
import { ChevronLeftIcon, ChevronRightIcon, GoogleIcon, PersonIcon } from "@/assets/icons";
import { Button, Input, Link, Modal, ModalBody, ModalContent, ModalFooter, ModalHeader, Spinner } from "@nextui-org/react";
import { clone } from "lodash";
import queryString from "query-string";
import { Controller, SubmitHandler, useForm } from "react-hook-form";
import toast from "react-hot-toast";

import { getApiErrorMessage, isApiError, useApi } from "@/lib/api";
import { cn } from "@/lib/utils";
import { PhoneInput } from "@/components/ui/phone-input";
import { PasswordInput } from "@/components/ui";

export type SignUpMethods = "credentials" | "google";

export interface SignUpProps {
  opened: boolean;
  onClose: () => void;
}

export interface SignUpInputs {
  firstName: string;
  lastName: string;
  username: string;
  password: string;
}

export const SignUpModal: React.FC<SignUpProps> = ({ opened, onClose, ...props }) => {
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [method, setMethod] = useState<SignUpMethods | null>(searchParams.get("method") as any);
  const api = useApi();
  const form = useForm<SignUpInputs>({
    defaultValues: {
      firstName: "",
      lastName: "",
      username: "",
      password: ""
    }
  });
  const formErrorsRef = useRef(clone(form.formState.errors));
  const formErrors = form.formState.isSubmitting ? formErrorsRef.current : (formErrorsRef.current = clone(form.formState.errors));
  const componentId = useId();

  const [state, setState] = useState<{ action: "idle" | "loading" | "submitting"; error?: any }>({ action: "idle", error: null });

  const onSignUp: SubmitHandler<SignUpInputs> = async (inputs) => {
    try {
      setState({ action: "submitting" });
      await api.signUp(inputs);
      setState({ action: "idle" });
      onClose();
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

  const onSignUpWith = async (provider: Exclude<SignUpMethods, "credentials">) => {
    try {
      setMethod(provider);
      await api.signInWith(provider);
      onClose();
    } catch (error) {
      setMethod(null);
      toast.error(getApiErrorMessage(error), { id: componentId });
    }
  };

  return (
    <>
      <Modal isOpen={opened} onClose={onClose}>
        <ModalContent>
          <ModalHeader className="flex flex-col gap-1">Create a new account</ModalHeader>
          <ModalBody>
            <form onSubmit={form.handleSubmit(onSignUp)}>
              <div className={cn("grid grid-cols-12 gap-x-5 gap-y-5 pb-2.5", method != "credentials" && "hidden")}>
                <Controller
                  control={form.control}
                  name="firstName"
                  render={({ field: { onChange, onBlur, value, ref } }) => (
                    <Input
                      ref={ref}
                      onChange={onChange}
                      onBlur={onBlur}
                      value={value}
                      validationState="valid"
                      errorMessage={formErrors.firstName?.message}
                      autoComplete="off"
                      label="First name"
                      className="col-span-6"
                    />
                  )}
                />
                <Controller
                  control={form.control}
                  name="lastName"
                  render={({ field: { onChange, onBlur, value, ref } }) => (
                    <Input
                      ref={ref}
                      onChange={onChange}
                      onBlur={onBlur}
                      value={value}
                      validationState="valid"
                      errorMessage={formErrors.lastName?.message}
                      autoComplete="off"
                      label="Last name"
                      className="col-span-6"
                    />
                  )}
                />
                <Controller
                  control={form.control}
                  name="username"
                  render={({ field: { onChange, onBlur, value, ref } }) => (
                    <PhoneInput
                      ref={ref}
                      onChange={onChange}
                      onBlur={onBlur}
                      value={value}
                      validationState="valid"
                      errorMessage={formErrors.username?.message}
                      autoComplete="off"
                      label="Email or phone number"
                      className="col-span-12"
                    />
                  )}
                />
                <Controller
                  control={form.control}
                  name="password"
                  render={({ field: { onChange, onBlur, value, ref } }) => (
                    <PasswordInput
                      ref={ref}
                      onChange={onChange}
                      onBlur={onBlur}
                      value={value}
                      validationState="valid"
                      errorMessage={formErrors.password?.message}
                      autoComplete="off"
                      label="Password"
                      className="col-span-12"
                    />
                  )}
                />
                <Button color="primary" className="col-span-12" type="submit" isLoading={state.action == "submitting"}>
                  Sign up
                </Button>
                <div className="col-span-12 flex items-center justify-center text-center text-sm">
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
                onPress={() => onSignUpWith("google")}
              >
                Continue with Google
              </Button>
            </div>
          </ModalBody>
          <ModalFooter className="flex items-center justify-center text-center text-sm">
            Already have an account?{" "}
            <Link as={NextLink} className="text-sm" href={queryString.stringifyUrl({ url: pathname, query: { dialogId: "sign-in", method: method ?? undefined } })}>
              Sign In <ChevronRightIcon className="h-4 w-4" />
            </Link>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </>
  );
};
