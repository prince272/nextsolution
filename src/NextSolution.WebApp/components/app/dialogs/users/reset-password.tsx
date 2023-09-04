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
import { useTimer } from "react-timer-hook";

import { getApiErrorMessage, isApiError, useApi } from "@/lib/api";
import { cn } from "@/lib/utils";
import { PhoneInput } from "@/components/ui/phone-input";
import { PasswordInput } from "@/components/ui";

export type ResetPasswordMethods = "credentials" | "google";

export interface ResetPasswordProps {
  opened: boolean;
  onClose: () => void;
}

export interface ResetPasswordInputs {
  username: string;
  code: string;
  password: string;
}

export const ResetPasswordModal: React.FC<ResetPasswordProps> = ({ opened, onClose, ...props }) => {
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const api = useApi();
  const form = useForm<ResetPasswordInputs>({
    defaultValues: {
      username: "",
      code: "",
      password: ""
    }
  });
  const formErrorsRef = useRef(clone(form.formState.errors));
  const formErrors = form.formState.isSubmitting ? formErrorsRef.current : (formErrorsRef.current = clone(form.formState.errors));
  const componentId = useId();

  const resendCodeTimer = useTimer({
    expiryTimestamp: new Date(new Date().getTime() + 59 * 1000),
    onExpire: () => {
      resendCodeTimer.restart(new Date(new Date().getTime() + 59 * 1000), false);
    },
    autoStart: false
  });

  const [state, setState] = useState<{ action: "idle" | "loading" | "submitting" | "sending"; error?: any }>({
    action: "idle",
    error: null
  });

  const onResetPassword: SubmitHandler<ResetPasswordInputs> = async (inputs) => {
    try {
      setState({ action: "submitting" });
      await api.resetPassword(inputs);
      toast.success("Password reset successfully!");
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

  const onSendCode: SubmitHandler<Exclude<ResetPasswordInputs, "password">> = async (inputs) => {
    try {
      setState({ action: "sending" });
      await api.resetPasswordCode(inputs);
      resendCodeTimer.start();
      toast.success("Password reset code sent!");
      setState({ action: "idle" });
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

  return (
    <>
      <Modal isOpen={opened} onClose={onClose}>
        <ModalContent>
          <ModalHeader className="flex flex-col gap-1">Reset your password</ModalHeader>
          <ModalBody>
            <form onSubmit={form.handleSubmit(onResetPassword)}>
              <div className={cn("grid gap-y-5 pb-2.5")}>
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
                    />
                  )}
                />
                <Controller
                  control={form.control}
                  name="code"
                  render={({ field: { onChange, onBlur, value, ref } }) => (
                    <Input
                      ref={ref}
                      onChange={onChange}
                      onBlur={onBlur}
                      value={value}
                      validationState="valid"
                      errorMessage={formErrors.code?.message}
                      autoComplete="off"
                      label="Code"
                      placeholder="Enter 6 digit code"
                      endContent={
                        <Button
                          className="-mt-4 px-7"
                          color="default"
                          variant="faded"
                          size="sm"
                          type="button"
                          isLoading={state.action == "sending"}
                          spinnerPlacement="end"
                          isDisabled={resendCodeTimer.isRunning}
                          onPress={() => form.handleSubmit(onSendCode)()}
                        >
                          {state.action == "sending" ? "Sending code..." : resendCodeTimer.isRunning ? `Resend code in ${resendCodeTimer.seconds}s` : "Get code"}
                        </Button>
                      }
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
                      label="New password"
                    />
                  )}
                />
                <Button color="primary" type="submit" isLoading={state.action == "submitting"}>
                  Reset password
                </Button>
              </div>
            </form>
          </ModalBody>
          <ModalFooter className="flex items-center justify-center text-center text-sm">
            Don&apos;t have an account?{" "}
            <Link as={NextLink} className="text-sm" href={queryString.stringifyUrl({ url: pathname, query: { dialogId: "sign-in" } })}>
              Sign in <ChevronRightIcon className="h-4 w-4" />
            </Link>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </>
  );
};
