"use client";

import { FC, useRef, useState } from "react";
import { usePathname } from "next/navigation";
import { PasswordInput } from "@/ui/password-input";
import { PhoneInput } from "@/ui/phone-input";
import { Button } from "@nextui-org/button";
import { Input } from "@nextui-org/input";
import { Modal, ModalBody, ModalContent, ModalHeader } from "@nextui-org/modal";
import { clone } from "lodash";
import { Controller, SubmitHandler, useForm } from "react-hook-form";
import toast from "react-hot-toast";
import { useTimer } from "react-timer-hook";
import { v4 as uuidv4 } from "uuid";

import { useApi } from "@/lib/api/client";
import { getErrorMessage, isApiError } from "@/lib/api/utils";
import { useConditionalState } from "@/lib/hooks";
import { cn } from "@/lib/utils";

export interface ResetPasswordProps {
  opened: boolean;
  onClose: (force?: boolean) => void;
}

export interface ResetPasswordInputs {
  username: string;
  code: string;
  password: string;
}

export const ResetPasswordModal: FC<ResetPasswordProps> = ({ opened, onClose }) => {
  const pathname = usePathname();
  const api = useApi();
  const form = useForm<ResetPasswordInputs>({
    defaultValues: {
      username: "",
      code: "",
      password: ""
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

  const onResetPassword: SubmitHandler<ResetPasswordInputs> = async (inputs) => {
    try {
      setStatus({ action: "submitting" });
      await api.post("/users/password/reset", inputs);

      try {
        await api.signIn(inputs);
      } catch {}

      toast.success("Password reset successful!");
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

  const onSendCode: SubmitHandler<Exclude<ResetPasswordInputs, "password">> = async (inputs) => {
    try {
      setStatus({ action: "sending" });
      await api.post("/users/password/reset/send-code", inputs);
      resendCodeTimer.start();
      toast.success("Password reset code sent!");
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
    <Modal isKeyboardDismissDisabled={true} isOpen={opened} onClose={() => onClose(false)} as={"form"} onSubmit={form.handleSubmit(onResetPassword)}>
      <ModalContent>
        <ModalHeader className="flex flex-col gap-1">Reset your password</ModalHeader>
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
                  label="New password"
                />
              )}
            />
            <Button color="primary" onPress={() => form.handleSubmit(onResetPassword)()} isLoading={status.action == "submitting"}>
              Continue
            </Button>
          </div>
        </ModalBody>
      </ModalContent>
    </Modal>
  );
};
