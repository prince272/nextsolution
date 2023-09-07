"use client";

import React from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Button } from "@nextui-org/button";
import queryString from "query-string";

import { useUser } from "@/lib/api/provider";
import { Sheet, SheetBody, SheetContent, SheetFooter, SheetHeader, useDisclosure } from "@/components/ui/sheet";
import { AppLoader } from "@/components/app/loader";

export default function About() {
  const router = useRouter();
  const { isOpen, onOpen, onOpenChange } = useDisclosure();

  const currentUser = useUser({
    onUnauthenticated: () => {
      router.push(queryString.stringifyUrl({ url: "/", query: { dialogId: "sign-in" } }));
    }
  });

  if (!currentUser) return <AppLoader loading={true} />;

  return (
    <>
      <Button onPress={onOpen}>Open Modal</Button>
      <Sheet backdrop="opaque" placement="left" size="sm" isOpen={isOpen} onOpenChange={onOpenChange}>
        <SheetContent>
          {(onClose) => (
            <>
              <SheetHeader className="flex flex-col gap-1">Modal Title</SheetHeader>
              <SheetBody>
                <p>
                  Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam pulvinar risus non risus hendrerit venenatis. Pellentesque sit amet hendrerit risus, sed porttitor
                  quam.
                </p>
              </SheetBody>
              <SheetFooter>
                <Button color="danger" variant="light" onPress={onClose}>
                  Close
                </Button>
                <Button color="primary" onPress={onClose}>
                  Action
                </Button>
              </SheetFooter>
            </>
          )}
        </SheetContent>
      </Sheet>
    </>
  );
}
