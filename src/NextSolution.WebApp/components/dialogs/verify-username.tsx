"use client";

import { FC, useEffect, useRef, useState } from "react";
import NextLink from "next/link";
import { usePathname, useSearchParams } from "next/navigation";
import { ChevronLeftIcon, ChevronRightIcon, GoogleIcon, PersonIcon } from "@/assets/icons";
import { PhoneInput } from "@/ui/phone-input";
import { Button } from "@nextui-org/button";
import { Input } from "@nextui-org/input";
import { Link } from "@nextui-org/link";
import { Modal, ModalBody, ModalContent, ModalFooter, ModalHeader } from "@nextui-org/modal";
import { clone } from "lodash";
import queryString from "query-string";
import { Controller, SubmitHandler, useForm } from "react-hook-form";
import toast from "react-hot-toast";
import { useTimer } from "react-timer-hook";
import { v4 as uuidv4 } from "uuid";

import { useApi, useUser } from "@/lib/api/client";
import { getApiErrorMessage, isApiError } from "@/lib/api/utils";
import { useConditionalState } from "@/lib/hooks";
import { cn } from "@/lib/utils";

export interface VerifyUsernameProps {
  opened: boolean;
  onClose: (force?: boolean) => void;
}

export type VerifyUsernameInputs = {
  username: string;
  usernameType: "email" | "phoneNumber";
  code: string;
  password: string;
};

export type UsernameType = {
  key: "email" | "phoneNumber";
  case: {
    lower: string;
    sentence: string;
    title: string;
    upper: string;
  };
};

const createVerifyUsernameModal = (usernameType: UsernameType) => {
  const VerifyUsernameModal: FC<VerifyUsernameProps> = ({ opened, onClose }) => {
    const searchParams = useSearchParams();
    const api = useApi();
    const form = useForm<VerifyUsernameInputs>({
      defaultValues: {
        username: searchParams.get("username")!,
        usernameType: usernameType.key,
        code: ""
      }
    });
    const formErrors = useConditionalState(clone(form.formState.errors), !form.formState.isSubmitting);
    const componentId = useRef(uuidv4()).current;

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

    const onVerifyUsername: SubmitHandler<VerifyUsernameInputs> = async (inputs) => {
      try {
        setState({ action: "submitting" });
        await api.verifyUsername(inputs);

        toast.success(`${usernameType.case.title} has been verified!`);
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

    const onSendCode: SubmitHandler<Exclude<VerifyUsernameInputs, "password">> = async (inputs) => {
      try {
        setState({ action: "sending" });
        await api.sendUsernameVerifyCode(inputs);
        resendCodeTimer.start();
        toast.success("Verification code sent!");
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
        <Modal isOpen={opened} onClose={() => onClose(false)}>
          <ModalContent>
            <ModalHeader className="flex flex-col gap-1">Verify your {usernameType.case.sentence}</ModalHeader>
            <ModalBody>
              <form onSubmit={form.handleSubmit(onVerifyUsername)}>
                <div className={cn("grid gap-y-5 pb-2.5")}>
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
                        label={usernameType.case.sentence}
                        isDisabled
                        countryVisibility={usernameType.key == "phoneNumber"}
                      />
                    )}
                  />
                  <Controller
                    control={form.control}
                    name="code"
                    render={({ field: { onChange, onBlur, value, ref } }) => (
                      <Input
                        ref={ref}
                        onChange={(e) => onChange(e.target.value)}
                        onBlur={onBlur}
                        value={value}
                        isInvalid={!!formErrors.code}
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
                  <Button color="primary" type="submit" isLoading={state.action == "submitting"}>
                    Continue
                  </Button>
                </div>
              </form>
            </ModalBody>
          </ModalContent>
        </Modal>
      </>
    );
  };

  return VerifyUsernameModal;
};

const VerifyEmailModal = createVerifyUsernameModal({
  key: "email",
  case: {
    lower: "email",
    sentence: "Email address",
    title: "Email Address",
    upper: "EMAIL ADDRESS"
  }
});

VerifyEmailModal.displayName = "VerifyEmailModal";

const VerifyPhoneNumberModal = createVerifyUsernameModal({
  key: "phoneNumber",
  case: {
    lower: "phone number",
    sentence: "Phone number",
    title: "Phone Number",
    upper: "PHONE NUMBER"
  }
});
VerifyPhoneNumberModal.displayName = "VerifyPhoneNumberModal";

export { VerifyEmailModal, VerifyPhoneNumberModal };
