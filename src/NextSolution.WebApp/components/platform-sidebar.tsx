"use client";

import { FC, useState } from "react";
import NextLink from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { PersonArrowRightIcon, SettingsIcon } from "@/assets/icons";
import { Button } from "@nextui-org/button";
import { Dropdown, DropdownItem, DropdownMenu, DropdownTrigger } from "@nextui-org/dropdown";
import { User } from "@nextui-org/user";
import queryString from "query-string";

import { useApi, useUser } from "@/lib/api/client";
import { useApp } from "@/components/provider";

import { Sheet, SheetBody, SheetContent, SheetFooter, SheetHeader } from "./ui/sheet";

const UserButton: FC = () => {
  const api = useApi();
  const pathname = usePathname();
  const currentUser = useUser();
  const [loading, setLoading] = useState(false);

  const onSignOut = async () => {
    try {
      setLoading(true);
      await api.signOut();
    } catch {
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      {currentUser && (
        <Dropdown>
          <DropdownTrigger>
            <Button variant="light" fullWidth isLoading={loading} className="mt-auto h-12 justify-start">
              <User
                name={`${currentUser.firstName} ${currentUser.lastName}`}
                description={`@${currentUser.userName}`}
                avatarProps={{
                  src: "https://i.pravatar.cc/150?u=a04258114e29026702d"
                }}
              />
            </Button>
          </DropdownTrigger>
          <DropdownMenu aria-label="User Actions">
            <DropdownItem showDivider key="settings" startContent={<SettingsIcon type="outlined" className="pointer-events-none h-6 w-6 flex-shrink-0 text-default-500" />}>
              <NextLink href={queryString.stringifyUrl({ url: pathname, query: { dialogId: "sign-in" } })}>Settings</NextLink>
            </DropdownItem>
            <DropdownItem
              key="sign-out"
              startContent={<PersonArrowRightIcon type="outlined" className="pointer-events-none h-6 w-6 flex-shrink-0 text-default-500" />}
              onPress={() => onSignOut()}
            >
              Sign out
            </DropdownItem>
          </DropdownMenu>
        </Dropdown>
      )}
    </>
  );
};

export const PlatformSidebar: FC = () => {
  const { sidebar } = useApp();

  return (
    <Sheet
      placement="left"
      classNames={{
        base: "max-w-[270px]"
      }}
      isSticky={true}
      isOpen={sidebar.opened}
      onOpenChange={(opened) => {
        (opened ? sidebar.open : sidebar.close)();
      }}
    >
      <SheetContent>
        <SheetHeader className="flex flex-col gap-1">Modal Title</SheetHeader>
        <SheetBody></SheetBody>
        <SheetFooter>
          <UserButton />
        </SheetFooter>
      </SheetContent>
    </Sheet>
  );
};
