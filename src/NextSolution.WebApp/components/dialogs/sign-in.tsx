"use client";

import React, { useState } from "react";
import NextLink from "next/link";
import { ChevronLeftIcon, GoogleIcon, PersonIcon } from "@/assets/icons";
import { Button, Input, Link, Modal, ModalBody, ModalContent, ModalFooter, ModalHeader } from "@nextui-org/react";
import { SubmitHandler, useForm } from "react-hook-form";

import { cn } from "@/lib/utils";
import { PasswordInput } from "@/components/ui";

export interface SignInProps {
  opened: boolean;
  onOpen: () => void;
  onClose: () => void;
}

export interface SignInInputs {
  username: string;
  password: string;
}

export const SignInModal: React.FC<SignInProps> = ({ opened, onOpen, onClose, ...props }) => {
  const [method, setMethod] = useState<"credentials" | "google" | null>(null);
  const form = useForm<SignInInputs>({
    defaultValues: {
      username: "",
      password: ""
    }
  });
  const formState = form.formState;

  const onSubmit: SubmitHandler<SignInInputs> = (inputs) => {};

  return (
    <>
      <Modal isOpen={opened} onOpenChange={(open) => (open ? onOpen() : onClose())}>
        <ModalContent>
          <ModalHeader className="flex flex-col gap-1">Sign in to your account</ModalHeader>
          <ModalBody>
            <form onSubmit={form.handleSubmit(onSubmit)}>
              <div className={cn("grid gap-y-5", method != "credentials" && "hidden")}>
                <Input {...form.register("username")} label="Email or phone number" />
                <PasswordInput {...form.register("password")} label="Password" />
                <Button color="primary">Sign in</Button>
                <div className="text-center flex justify-center items-center"><Link as="button" onPress={() => setMethod(null)}><ChevronLeftIcon className="w-4 h-4" /> Go back</Link></div>
              </div>
            </form>
            <div className={cn("grid gap-y-5", method && "hidden")}>
              <Button startContent={<PersonIcon className="h-6 w-6" />} color="primary" onPress={() => setMethod("credentials")}>
                Use email or phone
              </Button>
              <Button startContent={<GoogleIcon className="h-6 w-6" />}>Continue with Google</Button>
            </div>
          </ModalBody>
          <ModalFooter></ModalFooter>
        </ModalContent>
      </Modal>
    </>
  );
};
