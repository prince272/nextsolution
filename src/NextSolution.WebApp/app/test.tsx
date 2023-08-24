"use client";

import React, { useMemo, useState } from "react";
import NextLink from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { Button, Input, Modal, ModalBody, ModalContent, ModalHeader, ModalProps } from "@nextui-org/react";
import queryString from "query-string";

import { FileInput } from "@/components/ui";

import { useApi, useUser } from "./providers";

export default function Test() {
  const router = useRouter();
  const pathname = usePathname();
  const api = useApi();
  const user = useUser();
  const [value, setValue] = useState<string | string[]>("test.txt");

  return (
    <div className="mt-7 w-[500px]">
      <Button
        as={NextLink}
        className="mb-4"
        color="primary"
        href={queryString.stringifyUrl({ url: pathname, query: { dialogId: "sign-in" } })}
      >
        {!user ? "Sign In" : `You're currently signed in to ${user.firstName} ${user.lastName}`}
      </Button>
      <FileInput value={value} onChange={setValue} server="/files" />
    </div>
  );
}
