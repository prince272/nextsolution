import React from "react";
import { EyeHideIcon, EyeShowIcon } from "@/assets/icons";
import { Input, InputProps } from "@nextui-org/react";

const PasswordInput = React.forwardRef<HTMLInputElement, InputProps>((props, ref) => {
  const [isVisible, setIsVisible] = React.useState(false);
  const toggleVisibility = () => setIsVisible(!isVisible);

  return (
    <Input
      endContent={
        <button className="focus:outline-none" type="button" onClick={toggleVisibility}>
          {isVisible ? (
            <EyeHideIcon className="pointer-events-none h-6 w-6 text-default-400" />
          ) : (
            <EyeShowIcon className="pointer-events-none h-6 w-6 text-default-400" />
          )}
        </button>
      }
      {...props}
      type={isVisible ? props.type : "password"}
      ref={ref}
    />
  );
});
PasswordInput.displayName = "PasswordInput";

export { PasswordInput };
