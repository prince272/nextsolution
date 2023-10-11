"use client";

import { ElementRef, forwardRef, useEffect, useRef, useState } from "react";
import { Modal, ModalBody, ModalContent, ModalFooter, ModalHeader, ModalProps, useDisclosure } from "@nextui-org/modal";
import { ModalSlots, SlotsToClasses } from "@nextui-org/theme";

import { useBreakpoint } from "@/lib/hooks";
import { cn } from "@/lib/utils";

export interface SheetProps extends Omit<ModalProps, "placement"> {
  placement?: "left" | "right";
  isStatic?: boolean;
}

const Sheet = forwardRef<ElementRef<typeof Modal>, SheetProps>(
  ({ placement = "left", shouldBlockScroll, isOpen, isDismissable, isStatic = false, onOpenChange, classNames, ...props }, ref) => {
    const [mounted, setMounted] = useState(false);
    const containerRef = useRef<HTMLDivElement>(null);
    const md = useBreakpoint("md", "up");
    isStatic = mounted && md && isStatic;

    const extendedClassNames = {
      backdrop: cn(isStatic && "hidden", classNames?.backdrop),
      base: cn(isStatic && "w-screen", "!m-0 h-full !rounded-none", classNames?.base),
      body: cn("px-4", classNames?.body),
      closeButton: cn(classNames?.closeButton),
      footer: cn("px-4", classNames?.footer),
      header: cn("px-4", classNames?.header),
      wrapper: cn(isStatic && "w-auto relative", placement == "left" ? "!justify-start" : placement == "right" ? "justify-end" : "", classNames?.wrapper)
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
      if (!md) onOpenChange?.(false);
    }, [md]);

    useEffect(() => {
      // Feat: Workaround to ensure elements outside the sheet are focusable
      // Get all elements with a tabindex attribute
      const elementsWithTabIndex = containerRef.current?.querySelectorAll("*") || [];

      // Remove the tabindex attribute from each element
      elementsWithTabIndex.forEach((element) => {
        var attributes = element.attributes;

        // Iterate through the attributes and remove those that start with "aria"
        for (var j = 0; j < attributes.length; j++) {
          var attributeName = attributes[j].name;
          if (attributeName.startsWith("aria")) {
          }
          if (attributeName.startsWith("tabindex")) {
            element.removeAttribute(attributeName);
          }
        }
      });
    });

    return (
      <>
        <div ref={containerRef} className={cn(isStatic && "sticky top-0 h-screen")}></div>
        <Modal
          isKeyboardDismissDisabled={true}
          shouldBlockScroll={!isStatic && shouldBlockScroll}
          isOpen={isOpen}
          onOpenChange={onOpenChange}
          isDismissable={!isStatic && isDismissable}
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
