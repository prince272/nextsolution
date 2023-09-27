"use client";

import { ElementRef, forwardRef, useState } from "react";
import { EyeHideIcon, EyeShowIcon } from "@/assets/icons";
import { Input, InputProps } from "@nextui-org/input";

const PasswordInput = forwardRef<ElementRef<typeof Input>, InputProps>((props, ref) => {
  const [isVisible, setIsVisible] = useState(false);
  const toggleVisibility = () => setIsVisible(!isVisible);

  return (
    <Input
      endContent={
        <button className="focus:outline-none" type="button" onClick={toggleVisibility}>
          {isVisible ? <EyeHideIcon className="pointer-events-none h-5 w-5 text-default-400" /> : <EyeShowIcon className="pointer-events-none h-5 w-5 text-default-400" />}
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
