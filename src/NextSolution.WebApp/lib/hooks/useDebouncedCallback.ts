// source: https://github.com/react-restart/hooks/blob/master/src/useDebouncedCallback.ts

import { useCallback, type DependencyList } from "react";

import { useTimeout } from "./useTimeout";

/**
 * Creates a debounced function that will invoke the input function after the
 * specified delay.
 *
 * @param fn a function that will be debounced
 * @param delay The milliseconds delay before invoking the function
 */
export function useDebouncedCallback<TCallback extends (...args: any[]) => any>(
  fn: TCallback,
  dependencies: DependencyList,
  delay: number
): (...args: Parameters<TCallback>) => void {
  const timeout = useTimeout();
  return useCallback(
    (...args: any[]) => {
      timeout.set(() => {
        fn(...args);
      }, delay);
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [timeout, delay, fn, ...dependencies]
  );
}
