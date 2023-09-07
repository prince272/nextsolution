import { FC, forwardRef, HTMLProps, ReactNode, useRef } from "react";
import { Spinner } from "@nextui-org/spinner";

import { useForwardRef, useLockedBody } from "@/lib/hooks";
import { cn } from "@/lib/utils";

export interface AppLoaderProps extends HTMLProps<HTMLDivElement> {
  loading?: boolean;
}

const AppLoader = forwardRef<HTMLDivElement, AppLoaderProps>(({ children, loading, className, style, ...props }, forwardRef) => {
  const ref = useForwardRef(forwardRef);
 
  useLockedBody("auto", ref);

  return (
    <div ref={ref} className={cn(className, "h-full w-full")} style={{ overflow: "hidden", ...style }} {...props}>
      <div className={cn("flex h-full w-full flex-col items-center justify-center", !loading && "hidden")}>
        <Spinner size="lg" aria-label="Loading..." />
      </div>
      {children}
    </div>
  );
});

AppLoader.displayName = "AppLoader";

export { AppLoader };
