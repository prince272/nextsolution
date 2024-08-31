"use client";

import { ComponentProps } from "react";
import { uniqueId } from "lodash";
import { useTheme } from "next-themes";
import { Toaster as Sonner, toast } from "sonner";

export type ToasterProps = ComponentProps<typeof Sonner>;

export const Toaster = ({ ...props }: ToasterProps) => {
  const { theme } = useTheme();

  return (
    <Sonner
      richColors
      theme={theme as ToasterProps["theme"]}
      className="toaster group"
      position="top-center"
      toastOptions={{
        classNames: {
          toast:
            "group toast group-[.toaster]:bg-content1 group-[.toaster]:text-foreground group-[.toaster]:border-default-200 group-[.toaster]:shadow-lg",
          description: "group-[.toast]:text-default-500",
          actionButton: "group-[.toast]:bg-primary group-[.toast]:text-primary-foreground",
          cancelButton: "group-[.toast]:g-default group-[.toast]:text-default-foreground",
          error: "!bg-danger-50 !text-danger",
          success: "!bg-success-50 !text-success",
          warning: "!bg-warning-50 !text-warning",
          info: "!bg-primary-50 !text-primary"
        }
      }}
      {...props}
    />
  );
};

const toastId = uniqueId("toast-");
export { toast, toastId };
