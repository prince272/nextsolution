import { clone } from "lodash";
import { useMemo } from "react";

function useMemoizedValue<T>(value: T, shouldUpdate: boolean): T {
  const memoizedValue = useMemo(() => clone(value), [shouldUpdate]);
  return shouldUpdate ? memoizedValue : value;
}

export { useMemoizedValue };