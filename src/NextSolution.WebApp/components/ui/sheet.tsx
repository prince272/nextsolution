"use client";

import { ElementRef, forwardRef, LegacyRef, useEffect, useRef, useState } from "react";
import { Modal, ModalBody, ModalContent, ModalFooter, ModalHeader, ModalProps, useDisclosure } from "@nextui-org/modal";
import { ModalSlots, SlotsToClasses } from "@nextui-org/theme";

import { useForwardRef, useResponsive } from "@/lib/hooks";
import { cn } from "@/lib/utils";

export interface SheetProps extends Omit<ModalProps, "placement"> {
  placement?: "left" | "right";
  isSticky?: boolean;
}

const Sheet = forwardRef<ElementRef<typeof Modal>, SheetProps>(
  ({ placement = "left", shouldBlockScroll, isOpen, isDismissable, isSticky = false, onOpenChange, classNames, ...props }, ref) => {
    const [mounted, setMounted] = useState(false);
    const containerRef = useRef<HTMLDivElement>(null);
    const { md } = useResponsive();
    isSticky = mounted && md && isSticky;

    const extendedClassNames = {
      backdrop: cn(isSticky && "hidden", classNames?.backdrop),
      base: cn(isSticky && "w-screen", "!m-0 h-full !rounded-none", classNames?.base),
      body: cn("px-4", classNames?.body),
      closeButton: cn(classNames?.closeButton),
      footer: cn("px-4", classNames?.footer),
      header: cn("px-4", classNames?.header),
      wrapper: cn(isSticky && "w-auto relative", placement == "left" ? "!justify-start" : placement == "right" ? "justify-end" : "", classNames?.wrapper)
    } as SlotsToClasses<ModalSlots>;

    const motionProps =
      placement == "left"
        ? {
            initial: { x: -1000 },
            animate: { x: 0 },
            exit: { x: -1000 },
            transition: { duration: 0.4 }
          }
        : {
            initial: { x: 1000 },
            animate: { x: 0 },
            exit: { x: 1000 },
            transition: { duration: 0.4 }
          };

    useEffect(() => {
      setMounted(true);
    }, []);

    useEffect(() => {
      onOpenChange?.(false);
      // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [md]);

    return (
      <>
        <div ref={containerRef} className={cn(isSticky && "sticky top-0 h-screen")}></div>
        <Modal
          shouldBlockScroll={!isSticky && shouldBlockScroll}
          isOpen={mounted ? isSticky || isOpen : false}
          onOpenChange={onOpenChange}
          isDismissable={!isSticky && isDismissable}
          disableAnimation={isSticky}
          hideCloseButton={isSticky}
          classNames={extendedClassNames}
          motionProps={motionProps}
          portalContainer={containerRef.current!}
          {...props}
          ref={ref}
        />
      </>
    );
  }
);
Sheet.displayName = "Sheet";

export { Sheet };

export const SheetBody = ModalBody;

export const SheetContent = ModalContent;

export const SheetFooter = ModalFooter;

export const SheetHeader = ModalHeader;

export { useDisclosure };
