import { ComponentProps, ForwardedRef, forwardRef } from "react";
import { Text as PaperText, HelperText } from "react-native-paper";
import { cn } from "@/utils";
import { cssInterop } from "nativewind";

const BaseText = PaperText;

cssInterop(BaseText, { className: "style" });
cssInterop(HelperText, { className: "style" });

export interface TextProps extends ComponentProps<typeof BaseText> {
  pressedClassName?: string;
}

const Text = forwardRef<TextProps["ref"], TextProps>(
  ({ children, className, pressedClassName, ...props }: TextProps, ref: ForwardedRef<TextProps["ref"]>) => {
    const appliedClassName = cn(className);

    return (
      <BaseText className={appliedClassName} ref={ref as TextProps["ref"]} {...props}>
        {children}
      </BaseText>
    );
  }
);

export { Text, HelperText };
