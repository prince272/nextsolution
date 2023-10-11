"use client";

import { FC, useRef, useState } from "react";
import { useSearchParams } from "next/navigation";
import { PhoneInput } from "@/ui/phone-input";
import { Button } from "@nextui-org/button";
import { Input } from "@nextui-org/input";
import { Modal, ModalBody, ModalContent, ModalHeader } from "@nextui-org/modal";
import { clone, lowerCase, startCase, upperFirst } from "lodash";
import { Controller, SubmitHandler, useForm } from "react-hook-form";
import toast from "react-hot-toast";
import { useTimer } from "react-timer-hook";
import { v4 as uuidv4 } from "uuid";

import { useApi } from "@/lib/api/client";
import { getErrorMessage, isApiError } from "@/lib/api/utils";
import { useConditionalState } from "@/lib/hooks";
import { cn } from "@/lib/utils";

export interface VerifyUsernameProps {
  opened: boolean;
  onClose: (force?: boolean) => void;
}

export type VerifyUsernameInputs = {
  username: string;
  usernameType: UsernameType;
  code: string;
  password: string;
};

export type UsernameType = "emailAddress" | "phoneNumber";

const createVerifyUsernameModal = (usernameType: UsernameType) => {
  const VerifyUsernameModal: FC<VerifyUsernameProps> = ({ opened, onClose }) => {
    const searchParams = useSearchParams();
    const api = useApi();
    const form = useForm<VerifyUsernameInputs>({
      defaultValues: {
        username: searchParams.get("username")!,
        usernameType: ({ emailAdress: "email" } as any)[usernameType] || usernameType,
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

    const [status, setStatus] = useState<{ action: "idle" | "submitting" | "sending"; error?: any }>({
      action: "idle"
    });

    const onSubmit: SubmitHandler<VerifyUsernameInputs> = async (inputs) => {
      try {
        setStatus({ action: "submitting" });
        await api.post("/users/username/verify", inputs);

        toast.success(`${upperFirst(lowerCase(usernameType))} has been verified!`);
        onClose(false);
      } catch (error) {
        setStatus({ action: "idle", error });

        if (isApiError(error)) {
          if (error.response) {
            const fields = Object.entries<string[]>(error.response.data.errors || []);
            fields.forEach(([name, message]) => {
              form.setError(name as any, { message: message?.join("\n") });
            });
          }
        }

        toast.error(getErrorMessage(error), { id: componentId });
      }
    };

    const onSendCode: SubmitHandler<Exclude<VerifyUsernameInputs, "password">> = async (inputs) => {
      try {
        setStatus({ action: "sending" });
        await api.post("/users/username/verify/send-code", inputs);
        resendCodeTimer.start();
        toast.success("Verification code sent!");
        setStatus({ action: "idle" });
      } catch (error) {
        setStatus({ action: "idle", error });

        if (isApiError(error)) {
          if (error.response) {
            const fields = Object.entries<string[]>(error.response.data.errors || []);
            fields.forEach(([name, message]) => {
              form.setError(name as any, { message: message?.join("\n") });
            });
          }
        }

        toast.error(getErrorMessage(error), { id: componentId });
      }
    };

    return (
      <Modal isKeyboardDismissDisabled={true} isOpen={opened} onClose={() => onClose(false)} as="form" onSubmit={form.handleSubmit(onSubmit)}>
        <ModalContent>
          <ModalHeader className="flex flex-col gap-1">Verify your {startCase(usernameType)}</ModalHeader>
          <ModalBody>
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
                    label={upperFirst(lowerCase(usernameType))}
                    isDisabled
                    countryVisibility={usernameType == "phoneNumber"}
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
                        isLoading={status.action == "sending"}
                        spinnerPlacement="end"
                        isDisabled={resendCodeTimer.isRunning}
                        onPress={() => form.handleSubmit(onSendCode)()}
                      >
                        {status.action == "sending" ? "Sending code..." : resendCodeTimer.isRunning ? `Resend code in ${resendCodeTimer.seconds}s` : "Get code"}
                      </Button>
                    }
                  />
                )}
              />
              <Button color="primary" onPress={() => form.handleSubmit(onSubmit)()} isLoading={status.action == "submitting"}>
                Continue
              </Button>
            </div>
          </ModalBody>
        </ModalContent>
      </Modal>
    );
  };

  return VerifyUsernameModal;
};

const VerifyEmailModal = createVerifyUsernameModal("emailAddress");

VerifyEmailModal.displayName = "VerifyEmailModal";

const VerifyPhoneNumberModal = createVerifyUsernameModal("phoneNumber");
VerifyPhoneNumberModal.displayName = "VerifyPhoneNumberModal";

export { VerifyEmailModal, VerifyPhoneNumberModal };
