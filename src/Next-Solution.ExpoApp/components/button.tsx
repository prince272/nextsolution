import React, { ComponentProps, ComponentRef, forwardRef } from "react";
import { cssInterop } from "nativewind";
import { Button as BaseButton } from "react-native-paper";

cssInterop(BaseButton, { className: "style" });

export interface ButtonProps extends ComponentProps<typeof BaseButton> {
  className?: string;
}

export type ButtonRef = ComponentRef<typeof BaseButton>;

const Button = forwardRef<ButtonRef, ButtonProps>(({ ...props }, ref) => {
  return <BaseButton ref={ref} {...props} />;
});

export { Button };
