"use client";

import React from "react";
import NextLink from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { Button } from "@nextui-org/react";
import queryString from "query-string";

export default function Test() {
  const router = useRouter();
  const pathname = usePathname();

  return (
    <div className="mt-7">
      <Button as={NextLink} color="primary" href={queryString.stringifyUrl({ url: pathname, query: { dialogId: "sign-in" } })}>
        Open Dialog
      </Button>
    </div>
  );
}
