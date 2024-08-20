import { ComponentProps, ForwardedRef, forwardRef } from "react";
import { cn } from "@/utils";
import { cssInterop } from "nativewind";
import { HelperText, Text as PaperText } from "react-native-paper";

const BaseText = PaperText;

cssInterop(BaseText, { className: "style" });
cssInterop(HelperText, { className: "style" });

export interface TextProps extends ComponentProps<typeof BaseText> {
  pressedClassName?: string;
}

const Text = forwardRef<TextProps["ref"], TextProps>(
  (
    { children, className, pressedClassName, ...props }: TextProps,
    ref: ForwardedRef<TextProps["ref"]>
  ) => {
    const appliedClassName = cn(className);

    return (
      <BaseText className={appliedClassName} ref={ref as TextProps["ref"]} {...props}>
        {children}
      </BaseText>
    );
  }
);

export { Text, HelperText };
