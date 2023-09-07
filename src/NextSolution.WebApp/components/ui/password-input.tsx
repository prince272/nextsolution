"use client";

import { forwardRef, useState } from "react";
import { EyeHideIcon, EyeShowIcon } from "../icons";
import { Input, InputProps } from "@nextui-org/input";

const PasswordInput = forwardRef<HTMLInputElement, InputProps>((props, ref) => {
  const [isVisible, setIsVisible] = useState(false);
  const toggleVisibility = () => setIsVisible(!isVisible);

  return (
    <Input
      endContent={
        <button className="focus:outline-none" type="button" onClick={toggleVisibility}>
          {isVisible ? <EyeHideIcon className="pointer-events-none h-6 w-6 text-default-400" /> : <EyeShowIcon className="pointer-events-none h-6 w-6 text-default-400" />}
        </button>
      }
      type={isVisible ? props.type : "password"}
      {...props}
      ref={ref}
    />
  );
});
PasswordInput.displayName = "PasswordInput";

export { PasswordInput };
