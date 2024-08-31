import React, { useCallback, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useMemoizedValue } from "@/hooks";
import { identityService } from "@/services";
import { useAuthentication } from "@/states";
import { Icon } from "@iconify-icon/react";
import SolarAltArrowLeftOutline from "@iconify-icons/solar/alt-arrow-left-outline";
import { Button } from "@nextui-org/button";
import { Modal, ModalBody, ModalContent, ModalFooter, ModalHeader } from "@nextui-org/modal";
import { useForm } from "react-hook-form";
import { ValidationFailed } from "@/services/results";
import { CreateAccountForm } from "@/services/types";
import { ModalComponentProps, useModalController } from ".";

export interface SignUpModalProps extends ModalComponentProps {}

export const SignUpModal = ({ isOpen, id, ...props }: SignUpModalProps) => {
  const { modals, setModalId, clearModalId } = useModalController();

  const form = useForm<CreateAccountForm>();
  const [formSubmitting, setFormSubmitting] = useState(false);
  const formErrors = useMemoizedValue(form.formState.errors, !formSubmitting);

  const { setUser: setCurrentUser } = useAuthentication();

  const handleSignUp = useCallback(async () => {
    setFormSubmitting(true);
    return form.handleSubmit(async (inputs) => {
      const response = await identityService.signInAsync({ ...inputs });
      setFormSubmitting(false);

      if (!response.success) {
        if (response instanceof ValidationFailed) {
          const errorFields = Object.entries<string[]>(response.errors || []);

          errorFields.forEach(([name, message]) => {
            form.setError(name as keyof CreateAccountForm, { message: message?.join("\n") });
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

  return (
    <>
      <Modal
        isOpen={isOpen}
        onOpenChange={(opened) => {
          if (!opened) clearModalId();
        }}
      >
        <ModalContent>
          <ModalHeader className="flex flex-col gap-1 pl-4">
            <div className="flex h-8 items-center space-x-1">
              <Button size="sm" variant="light" isIconOnly onPress={() => {}}>
                <Icon icon={SolarAltArrowLeftOutline} width="24" height="24" />
              </Button>
              <div>Sign into your account</div>
            </div>
          </ModalHeader>
          <ModalBody>
            <p>
              Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam pulvinar risus non
              risus hendrerit venenatis. Pellentesque sit amet hendrerit risus, sed porttitor quam.
            </p>
            <p>
              Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam pulvinar risus non
              risus hendrerit venenatis. Pellentesque sit amet hendrerit risus, sed porttitor quam.
            </p>
            <p>
              Magna exercitation reprehenderit magna aute tempor cupidatat consequat elit dolor
              adipisicing. Mollit dolor eiusmod sunt ex incididunt cillum quis. Velit duis sit
              officia eiusmod Lorem aliqua enim laboris do dolor eiusmod. Et mollit incididunt nisi
              consectetur esse laborum eiusmod pariatur proident Lorem eiusmod et. Culpa deserunt
              nostrud ad veniam.
            </p>
          </ModalBody>
          <ModalFooter>
            <Button
              color="danger"
              variant="light"
              onPress={() => {

              }}
            >
              Close
            </Button>
            <Button
              color="primary"
              onPress={() => {

              }}
            >
              Sign Up
            </Button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </>
  );
};
