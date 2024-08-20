import React, { ComponentProps, ComponentRef, forwardRef } from "react";
import { TextInput as RNTextInput } from "react-native";
import { TextInput as BaseTextInput } from "react-native-paper";

export interface TextInputProps extends ComponentProps<typeof BaseTextInput> {
  className?: string;
}

export type TextInputRef = ComponentRef<typeof RNTextInput>;

const TextInput = forwardRef<TextInputRef, TextInputProps>(({ ...props }, ref) => {
  return <BaseTextInput ref={ref} {...props} />;
});

export { TextInput };
