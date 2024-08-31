"use client";

import { useRouter } from "next/navigation";
import { ModalController } from "@/modals";
import { SignInModal } from "@/modals/sign-in-modal";
import { SignUpModal } from "@/modals/sign-up-modal";
import { Button } from "@nextui-org/button";
import { useTheme } from "next-themes";

export default function Page() {
  const router = useRouter();

  return (
    <div className="flex justify-center items-center h-screen">
      <Button color="primary" onClick={() => router.push("#sign-in-method")}>
        Sign In Method
      </Button>
      <Button color="primary" onClick={() => router.push("#sign-up")}>
        Sign Up
      </Button>
    </div>
  );
}
