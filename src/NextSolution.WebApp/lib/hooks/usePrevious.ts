import { useEffect, useRef } from "react";
import type { MutableRefObject } from "react";

export const usePrevious = <TValue extends unknown>(value: TValue, isEqual?: (prev: TValue, next: TValue) => boolean) => {
  const ref = useRef<{ value: TValue; prev: TValue | null }>({
    value: value,
    prev: null
  });

  const current = ref.current.value;

  if (isEqual ? !isEqual(current, value) : value !== current) {
    ref.current = {
      value: value,
      prev: current
    };
  }

  return ref.current.prev;
};
