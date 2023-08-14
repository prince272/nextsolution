"use client";

import React from "react";
import { usePathname, useRouter } from "next/navigation";
import { Button, Modal, ModalBody, ModalContent, ModalFooter, ModalHeader } from "@nextui-org/react";
import queryString from "query-string";

export interface SignUpProps {
  opened: boolean;
  onOpen: () => void;
  onClose: () => void;
}

export const SignUpModal: React.FC<SignUpProps> = ({ opened, onOpen, onClose, ...props }) => {
  const router = useRouter();
  const pathname = usePathname();

  return (
    <>
      <Modal isOpen={opened} onOpenChange={(open) => (open ? onOpen() : onClose())}>
        <ModalContent>
          <ModalHeader className="flex flex-col gap-1">Sign Up</ModalHeader>
          <ModalBody>
            <p>
              Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam pulvinar risus non risus hendrerit venenatis. Pellentesque sit
              amet hendrerit risus, sed porttitor quam.
            </p>
            <p>
              Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam pulvinar risus non risus hendrerit venenatis. Pellentesque sit
              amet hendrerit risus, sed porttitor quam.
            </p>
            <p>
              Magna exercitation reprehenderit magna aute tempor cupidatat consequat elit dolor adipisicing. Mollit dolor eiusmod sunt ex
              incididunt cillum quis. Velit duis sit officia eiusmod Lorem aliqua enim laboris do dolor eiusmod. Et mollit incididunt nisi
              consectetur esse laborum eiusmod pariatur proident Lorem eiusmod et. Culpa deserunt nostrud ad veniam.
            </p>
          </ModalBody>
          <ModalFooter>
            <Button color="danger" variant="light" onClick={onClose}>
              Close
            </Button>
            <Button
              color="primary"
              onClick={() => {
                router.push(queryString.stringifyUrl({ url: pathname, query: { dialogId: "sign-in" } }));
              }}
            >
              Action
            </Button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </>
  );
};
