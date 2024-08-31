import { useMemo } from "react";
import { clone } from "lodash";

export const useMemoizedValue = <T>(value: T, allowEffect: boolean): T => {
  const memoizedValue = useMemo(() => clone(value), [allowEffect]);
  return allowEffect ? value : memoizedValue;
};
