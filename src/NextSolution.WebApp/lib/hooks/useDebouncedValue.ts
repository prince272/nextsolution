// source: https://github.com/react-restart/hooks/blob/master/src/useDebouncedValue.ts

import { useDebugValue, useEffect } from "react";

import { useDebouncedState } from "./useDebouncedState";

/**
 * Debounce a value change by a specified number of milliseconds. Useful
 * when you want need to trigger a change based on a value change, but want
 * to defer changes until the changes reach some level of infrequency.
 *
 * @param value
 * @param delayMs
 * @returns
 */
export function useDebouncedValue<TValue>(value: TValue, delayMs = 500): TValue {
  const [debouncedValue, setDebouncedValue] = useDebouncedState(value, delayMs);

  useDebugValue(debouncedValue);

  useEffect(() => {
    setDebouncedValue(value);
  }, [value, delayMs, setDebouncedValue]);

  return debouncedValue;
}
