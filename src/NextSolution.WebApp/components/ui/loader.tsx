"use client";

import { forwardRef, HTMLProps } from "react";
import { Spinner } from "@nextui-org/spinner";

import { useForwardRef, useLockedBody } from "@/lib/hooks";
import { cn } from "@/lib/utils";

export interface LoaderProps extends HTMLProps<HTMLDivElement> {
  loading?: boolean;
}

const Loader = forwardRef<HTMLDivElement, LoaderProps>(({ children, loading, style, className, ...props }, forwardRef) => {
  const ref = useForwardRef(forwardRef);

  useLockedBody(loading, "", ref);

  return (
    <div className={cn("h-full", loading && "[&>*:not(:first-child)]:hidden", className)} style={{ overflow: loading ? "hidden" : undefined, ...style }} {...props} ref={ref}>
      {loading && <Spinner className={cn("flex h-full flex-none items-center justify-center")} size="lg" aria-label="Loading..." />}
      {children}
    </div>
  );
});

Loader.displayName = "Loader";

export { Loader };
