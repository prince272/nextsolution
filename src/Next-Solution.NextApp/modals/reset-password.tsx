import React, { useCallback, useState } from "react";
import NextLink from "next/link";
import { usePathname, useSearchParams } from "next/navigation";
import { useMemoizedValue, useTimer } from "@/hooks";
import { identityService } from "@/services";
import { useAuthentication } from "@/states";
import { buildUrl } from "@/utils";
import { Icon } from "@iconify-icon/react";
import SolarAltArrowLeftOutline from "@iconify-icons/solar/alt-arrow-left-outline";
import { Button } from "@nextui-org/button";
import { Input } from "@nextui-org/input";
import { Link } from "@nextui-org/link";
import { Modal, ModalBody, ModalContent, ModalHeader } from "@nextui-org/modal";
import { Controller as FormController, useForm } from "react-hook-form";
import { toast } from "sonner";
import { ValidationFailed } from "@/services/results";
import { ResetPasswordForm } from "@/services/types";
import { PasswordInput } from "@/components/password-input";
import { ModalComponentProps, useModalController } from ".";

export interface ResetPasswordModalProps extends ModalComponentProps {}

export const ResetPasswordModal = ({ isOpen, id, ...props }: ResetPasswordModalProps) => {
  const { modals, setModalId, clearModalId } = useModalController();

  const form = useForm<ResetPasswordForm>();
  const [formSubmitting, setFormSubmitting] = useState(false);
  const formErrors = useMemoizedValue(form.formState.errors, !formSubmitting);

  const { setUser: setCurrentUser } = useAuthentication();

  const pathname = usePathname();
  const searchParams = useSearchParams();

  const [codeSending, setCodeSending] = useState(false);
  const [codeSent, setCodeSent] = useState(false);

  const sendCodeTimer = useTimer({
    timerType: "DECREMENTAL",
    initialTime: 60,
    endTime: 0
  });

  const sendResetPasswordCode = async () => {
    setCodeSending(true);

    return form.handleSubmit(async (inputs) => {
      const response = await identityService.sendResetPasswordCodeAsync(inputs);
      setCodeSending(false);

      if (!response.success) {
        if (response instanceof ValidationFailed) {
          const errorFields = Object.entries(response.errors || {});

          errorFields.forEach(([name, message]) => {
            form.setError(name as keyof ResetPasswordForm, { message: message?.join("\n") });
          });

          if (errorFields.length > 0) return;

          toast.error(response.message);
          return;
        } else {
          toast.error(response.message);
          return;
        }
      }

      toast.success(codeSent ? "Verification code resent!" : "Verification code sent!");
      sendCodeTimer.start();
      setCodeSent(true);
    })();
  };

  const handleResetPassword = useCallback(async () => {
    setFormSubmitting(true);
    return form.handleSubmit(async (inputs) => {
      const response = await identityService.resetPasswordAsync({ ...inputs });
      setFormSubmitting(false);

      if (!response.success) {
        if (response instanceof ValidationFailed) {
          const errorFields = Object.entries<string[]>(response.errors || []);

          errorFields.forEach(([name, message]) => {
            form.setError(name as keyof ResetPasswordForm, { message: message?.join("\n") });
          });

          if (errorFields.length > 0) return;

          toast.error(response.message);
          return;
        } else {
          toast.error(response.message);
          return;
        }
      }

      console.log("User signed in:", response.data.userName);
      setCurrentUser(response.data);
      setModalId("sign-in");
    })();
  }, [setModalId, form, setCurrentUser]);

  return (
    <>
      <Modal
        isOpen={isOpen}
        onOpenChange={(opened) => {
          if (!opened) clearModalId();
        }}
      >
        <ModalContent>
          <ModalHeader className="flex flex-col gap-1 pl-3">
            <div className="flex h-8 items-center space-x-1">
              <Button
                size="sm"
                variant="light"
                isIconOnly
                as={NextLink}
                href={buildUrl({
                  url: pathname,
                  query: Object.fromEntries(searchParams.entries()),
                  fragmentIdentifier: "sign-in"
                })}
              >
                <Icon icon={SolarAltArrowLeftOutline} width="24" height="24" />
              </Button>
              <div>Reset Your Password</div>
            </div>
          </ModalHeader>
          <ModalBody>
            <div className="grid grid-cols-12 gap-x-3 gap-y-5 pb-4">
              <FormController
                control={form.control}
                name="username"
                render={({ field }) => (
                  <Input
                    {...field}
                    className="col-span-12"
                    label="Email or phone number"
                    isInvalid={!!formErrors.username}
                    errorMessage={formErrors.username?.message}
                  />
                )}
              />
              <FormController
                control={form.control}
                name="code"
                render={({ field }) => (
                  <Input
                    {...field}
                    className="col-span-12"
                    label="Enter code"
                    description={
                      <span className="text-default-400">
                        {sendCodeTimer.isRunning
                          ? `Didn't get the code? Try again in ${sendCodeTimer.time}s.`
                          : codeSent
                            ? "Enter the code that was sent to you."
                            : "Request a code to be sent to you."}
                      </span>
                    }
                    isInvalid={!!formErrors.code}
                    errorMessage={formErrors.code?.message}
                    endContent={
                      <Button
                        className="-mt-4 px-7"
                        color="default"
                        variant="solid"
                        size="sm"
                        type="button"
                        spinnerPlacement="end"
                        isDisabled={codeSending || sendCodeTimer.isRunning}
                        isLoading={codeSending}
                        onPress={() => {
                          sendResetPasswordCode();
                        }}
                      >
                        {formSubmitting ? "Requesting code..." : "Request code"}
                      </Button>
                    }
                  />
                )}
              />
              <Button
                className="col-span-12"
                color="primary"
                type="button"
                variant="solid"
                isDisabled={!codeSent || formSubmitting}
                isLoading={formSubmitting}
                onPress={() => handleResetPassword()}
              >
                Reset password
              </Button>
            </div>
          </ModalBody>
        </ModalContent>
      </Modal>
    </>
  );
};
