import React, { useCallback, useState } from "react";
import NextLink from "next/link";
import { usePathname, useSearchParams } from "next/navigation";
import { WebBrowser } from "@/libs/web-browser";
import { identityService } from "@/services";
import { useAuthentication } from "@/states";
import { buildUrl, sleep } from "@/utils";
import { Icon } from "@iconify-icon/react";
import FlatColorIconsGoogle from "@iconify-icons/flat-color-icons/google";
import SolarUserBold from "@iconify-icons/solar/user-bold";
import { Button } from "@nextui-org/button";
import { Modal, ModalBody, ModalContent } from "@nextui-org/modal";
import queryString from "query-string";
import { toast } from "sonner";
import { SignInWithProvider } from "@/services/types";
import { ModalComponentProps, useModalController } from ".";

export interface SignInMethodModalProps extends ModalComponentProps {}

export const SignInMethodModal = ({ isOpen, id, ...props }: SignInMethodModalProps) => {
  const { modals, setModalId, clearModalId } = useModalController();
  const [signingInWith, setSigningInWith] = useState<SignInWithProvider | null>(null);

  const { setUser: setCurrentUser } = useAuthentication();

  const pathname = usePathname();
  const searchParams = useSearchParams();

  const handleSignInWith = useCallback(
    async (provider: SignInWithProvider) => {
      try {
        setSigningInWith(provider);
        await sleep(500);

        const callbackUrl = window.location.href;
        const redirectUrl = identityService.signInWithRedirect(provider, callbackUrl);
        const { linkingUrl } = await WebBrowser.open(redirectUrl, { center: true });
        const { query: queryParams } = queryString.parseUrl(linkingUrl);
        const { token } = queryParams || {};

        if (token) {
          const response = await identityService.SignInWithAsync(provider, token as string);

          if (!response.success) {
            console.log("Sign in failed:", response);
            toast.error(response.message);
            return;
          } else {
            console.log("User signed in:", response.data.userName);
            setCurrentUser(response.data);
            clearModalId();
          }
        }
      } catch (error) {
        console.error(error);
      } finally {
        setSigningInWith(null);
      }
    },
    [clearModalId, setCurrentUser]
  );

  return (
    <>
      <Modal
        isOpen={isOpen}
        onOpenChange={(opened) => {
          if (!opened) clearModalId();
        }}
      >
        <ModalContent>
          <ModalBody>
            <div className="flex flex-col items-center text-center space-x-1 p-4 flex-initial">
              <div className="text-large font-semibold">Welcome to Next Solution</div>
              <div className="text-sm text-default-500">
                Kickstart your web app with this template
              </div>
            </div>
            <div className="grid grid-cols-12 gap-x-3 gap-y-5 pb-4">
              <Button
                className="col-span-12"
                type="button"
                color="primary"
                variant="solid"
                startContent={<Icon icon={SolarUserBold} width="24" height="24" />}
                isDisabled={!!signingInWith}
                as={NextLink}
                href={buildUrl({
                  url: pathname,
                  query: Object.fromEntries(searchParams.entries()),
                  fragmentIdentifier: "sign-in"
                })}
              >
                Sign in with email or phone
              </Button>
              <Button
                className="col-span-12 light"
                type="button"
                color="default"
                variant="solid"
                startContent={
                  !signingInWith && <Icon icon={FlatColorIconsGoogle} width="24" height="24" />
                }
                isDisabled={signingInWith == "Google"}
                isLoading={signingInWith == "Google"}
                onPress={() => handleSignInWith("Google")}
              >
                Continue with Google
              </Button>
              <Button
                className="col-span-12"
                type="button"
                color="default"
                variant="flat"
                isDisabled={!!signingInWith}
                as={NextLink}
                href={buildUrl({
                  url: pathname,
                  query: Object.fromEntries(searchParams.entries()),
                  fragmentIdentifier: "sign-up"
                })}
              >
                Don&apos;t have an account? <span className="text-primary">Sign up</span>
              </Button>
            </div>
          </ModalBody>
        </ModalContent>
      </Modal>
    </>
  );
};
