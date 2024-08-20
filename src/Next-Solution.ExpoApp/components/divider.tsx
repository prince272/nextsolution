import React, { ComponentProps, ComponentRef, forwardRef } from "react";
import { View } from "react-native";
import { cn } from "@/utils";

export interface DividerProps extends ComponentProps<typeof View> {
  children?: React.ReactNode;
  alignment?: "left" | "center" | "right";
}

export type DividerRef = ComponentRef<typeof View>;

const Divider = forwardRef<View, DividerProps>(
  ({ children, className, alignment = "center", ...props }, ref) => {
    // Define the alignment class based on the `alignment` prop
    const alignmentStyles =
      {
        left: "items-start",
        center: "items-center",
        right: "items-end"
      }[alignment] || "items-center"; // Default to center if alignment is invalid

    return (
      <View
        ref={ref}
        className={cn("flex-row items-center py-6", className)}
        {...props} // Spread additional props
      >
        {(alignment === "center" || alignment === "left") && (
          <View className="flex-1 h-px bg-outline-variant" />
        )}
        <View className={`flex-none ${alignmentStyles}`}>{children}</View>
        {(alignment === "center" || alignment === "right") && (
          <View className="flex-1 h-px bg-outline-variant" />
        )}
      </View>
    );
  }
);

export { Divider };
