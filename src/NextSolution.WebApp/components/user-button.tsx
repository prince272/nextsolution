"use client";

import { FC, useState } from "react";
import NextLink from "next/link";
import { usePathname } from "next/navigation";
import { DarkThemeIcon, PersonArrowRightIcon, SettingsIcon } from "@/assets/icons";
import { Button } from "@nextui-org/button";
import { Dropdown, DropdownItem, DropdownMenu, DropdownTrigger } from "@nextui-org/dropdown";
import { User as UserAvatar } from "@nextui-org/user";
import { useTheme } from "next-themes";
import queryString from "query-string";

import { useApi, useUser } from "@/lib/api/client";
import { User } from "@/lib/api/types";

export const UserButton: FC = () => {
  const api = useApi();
  const pathname = usePathname();
  const currentUser = useUser() ?? ({} as User);
  const [loading, setLoading] = useState(false);
  const theme = useTheme();

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
      <Dropdown>
        <DropdownTrigger>
          <Button variant="light" fullWidth isLoading={loading} className="mt-auto h-12 justify-start">
            <UserAvatar
              name={`${currentUser.firstName} ${currentUser.lastName}`}
              description={`@${currentUser.userName}`}
              avatarProps={{
                src: currentUser.avatarUrl,
                showFallback: !currentUser.avatarUrl,
                fallback: <span className="text-xl">{currentUser.firstName?.[0]}</span>
              }}
            />
          </Button>
        </DropdownTrigger>
        <DropdownMenu aria-label="Account Actions">
          <DropdownItem
            isReadOnly
            key="theme"
            className="relative cursor-default"
            startContent={<DarkThemeIcon type="outlined" className="pointer-events-none h-5 w-5 flex-shrink-0 text-default-500" />}
            endContent={
              <select
                className="stretched-link z-10 w-16 rounded-md border-small border-default-300 bg-transparent py-0.5 text-tiny text-default-500 outline-none group-data-[hover=true]:border-default-500 dark:border-default-200"
                name="theme"
                title="Change theme"
                value={theme.theme}
                onChange={(e) => theme.setTheme(e.target.value)}
              >
                <option value="system">System</option>
                <option value="dark">Dark</option>
                <option value="light">Light</option>
              </select>
            }
          >
            Theme
          </DropdownItem>
          <DropdownItem
            showDivider
            key="settings"
            className="relative"
            startContent={<SettingsIcon type="outlined" className="pointer-events-none h-5 w-5 flex-shrink-0 text-default-500" />}
          >
            <NextLink href={queryString.stringifyUrl({ url: pathname, query: { dialogId: "account-settings" } })} className="stretched-link">
              Settings
            </NextLink>
          </DropdownItem>
          <DropdownItem
            key="sign-out"
            startContent={<PersonArrowRightIcon type="outlined" className="pointer-events-none h-5 w-5 flex-shrink-0 text-default-500" />}
            onPress={() => onSignOut()}
          >
            Sign out
          </DropdownItem>
        </DropdownMenu>
      </Dropdown>
    </>
  );
};
