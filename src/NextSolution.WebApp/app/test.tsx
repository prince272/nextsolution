"use client";

import React, { useMemo, useState } from "react";
import NextLink from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { Button, Input, Modal, ModalBody, ModalContent, ModalHeader, ModalProps } from "@nextui-org/react";
import queryString from "query-string";

import { FileInput } from "@/components/ui";

import { useSignalR, useSignalREffect } from "../components/app";
import { getApi } from "@/lib/api";

export default function Test() {
  const router = useRouter();
  const pathname = usePathname();
    const api = getApi();
    const [{ user }] = api.store.useState();
  const [value, setValue] = useState<string | string[]>("test.txt");

  const signalR = useSignalR();
  const [messages, setMessage] = useState<any[]>([]);

  useSignalREffect(
    "UserConnected",
    (user) => {
      setMessage(user);
      console.log(user);
    },
    [messages]
  );

  return (
    <div className="mt-7 w-[500px]">
      <Button
        as={NextLink}
        className="mb-4"
        color="primary"
        href={queryString.stringifyUrl({ url: pathname, query: { dialogId: "sign-in" } })}
      >
        {!user ? "Sign In" : `You're currently signed in to ${user.firstName} ${user.lastName}`}
        {messages.length}
      </Button>
      <FileInput value={value} onChange={setValue} server="/files" />
    </div>
  );
}
