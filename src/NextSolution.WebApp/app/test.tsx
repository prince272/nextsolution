"use client";

import React, { useMemo, useState } from "react";
import NextLink from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { Button, Input, Modal, ModalBody, ModalContent, ModalHeader, ModalProps } from "@nextui-org/react";
import queryString from "query-string";

import { useUser } from "./providers";

export default function Test() {
  const router = useRouter();
  const pathname = usePathname();
  const user = useUser();

  return (
    <div className="mt-7">
      <Button as={NextLink} color="primary" href={queryString.stringifyUrl({ url: pathname, query: { dialogId: "sign-in" } })}>
        {!user ? "Sign In" : `You're currently signed in to ${user.firstName} ${user.lastName}`}
      </Button>
    </div>
  );
}
