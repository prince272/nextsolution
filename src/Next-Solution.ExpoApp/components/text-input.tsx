import React, { ComponentProps, ComponentRef, forwardRef } from "react";
import { TextInput as RNTextInput } from "react-native";
import { TextInput as BaseTextInput } from "react-native-paper";

export interface TextInputProps extends ComponentProps<typeof BaseTextInput> {
  className?: string;
}


const TextInput = forwardRef<RNTextInput, TextInputProps>(({ className, ...props }, ref) => {
  return <BaseTextInput ref={ref} {...props} />;
});

export { TextInput };
