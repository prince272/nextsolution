// source: https://github.com/react-restart/hooks/blob/master/src/useEventCallback.ts

import { useCallback } from "react";

import { useCommittedRef } from "./useCommittedRef";

export function useEventCallback<TCallback extends (...args: any[]) => any>(fn?: TCallback | null): TCallback {
  const ref = useCommittedRef(fn);
  return useCallback(
    function (...args: any[]) {
      return ref.current && ref.current(...args);
    },
    [ref]
  ) as any;
}
