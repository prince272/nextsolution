import React, { useCallback, useEffect, useState } from "react";
import NextLink from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useMemoizedValue } from "@/hooks";
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
import { CreateAccountForm } from "@/services/types";
import { PasswordInput } from "@/components/password-input";
import { ModalComponentProps, useModalController } from ".";

export interface SignUpModalProps extends ModalComponentProps {}

export const SignUpModal = ({ isOpen, id, ...props }: SignUpModalProps) => {
  const { modals, setModalId, clearModalId } = useModalController();

  const form = useForm<CreateAccountForm>();
  const [formSubmitting, setFormSubmitting] = useState(false);
  const formErrors = useMemoizedValue(form.formState.errors, !formSubmitting);

  const { setUser: setCurrentUser } = useAuthentication();

  const pathname = usePathname();
  const searchParams = useSearchParams();

  const handleSignUp = useCallback(async () => {
    setFormSubmitting(true);
    return form.handleSubmit(async (inputs) => {
      const response = await identityService.createAccountAsync({ ...inputs });
      setFormSubmitting(false);

      if (!response.success) {
        if (response instanceof ValidationFailed) {
          const errorFields = Object.entries<string[]>(response.errors || []);

          errorFields.forEach(([name, message]) => {
            form.setError(name as keyof CreateAccountForm, { message: message?.join("\n") });
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
      clearModalId();
    })();
  }, [clearModalId, form, setCurrentUser]);

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
                  fragmentIdentifier: "sign-in-method"
                })}
              >
                <Icon icon={SolarAltArrowLeftOutline} width="24" height="24" />
              </Button>
              <div>Create Your New Account</div>
            </div>
          </ModalHeader>
          <ModalBody>
            <div className="grid grid-cols-12 gap-x-3 gap-y-5 pb-4">
              <FormController
                control={form.control}
                name="firstName"
                render={({ field }) => (
                  <Input
                    {...field}
                    className="col-span-6"
                    label="First name"
                    isInvalid={!!formErrors.firstName}
                    errorMessage={formErrors.firstName?.message}
                  />
                )}
              />
              <FormController
                control={form.control}
                name="lastName"
                render={({ field }) => (
                  <Input
                    {...field}
                    className="col-span-6"
                    label="Last name"
                    isInvalid={!!formErrors.lastName}
                    errorMessage={formErrors.lastName?.message}
                  />
                )}
              />
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
                name="password"
                render={({ field }) => (
                  <div className="col-span-12">
                    <PasswordInput
                      {...field}
                      label="Password"
                      isInvalid={!!formErrors.password}
                      errorMessage={formErrors.password?.message}
                    />
                  </div>
                )}
              />
              <Button
                className="col-span-12"
                color="primary"
                type="button"
                variant="solid"
                isDisabled={formSubmitting}
                isLoading={formSubmitting}
                onPress={() => handleSignUp()}
              >
                Create account
              </Button>
            </div>
          </ModalBody>
        </ModalContent>
      </Modal>
    </>
  );
};
