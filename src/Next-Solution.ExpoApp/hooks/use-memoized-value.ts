import { clone } from "lodash";
import { useMemo } from "react";

export const useMemoizedValue = <T>(value: T, allowEffect: boolean): T => {
  const memoizedValue = useMemo(() => clone(value), [allowEffect]);
  return allowEffect ? value : memoizedValue;
};