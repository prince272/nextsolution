import { useRef } from "react";

export function usePreviousValue<TValue extends unknown>(
  value: TValue,
  isEqual?: (prev: TValue, next: TValue) => boolean
) {
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
}
