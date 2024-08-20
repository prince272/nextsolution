import { useCallback } from "react";
import type { DependencyList } from "react";
import { useTimeout } from "./use-timeout";

/**
 * Creates a debounced function that will invoke the input function after the
 * specified delay.
 *
 * @param fn a function that will be debounced
 * @param dependencies An array of dependencies that will trigger re-creation of the debounced function
 * @param delay The milliseconds delay before invoking the function
 * @returns A debounced function
 */
export const useDebouncedCallback = <TCallback extends (...args: any[]) => any>(
  fn: TCallback,
  dependencies: DependencyList,
  delay: number
): ((...args: Parameters<TCallback>) => Promise<void>) => {
  const timeout = useTimeout();

  return useCallback(
    (...args: Parameters<TCallback>) => {
      return new Promise<void>((resolve) => {
        timeout.set(() => {
          fn(...args);
          resolve();
        }, delay);
      });
    },
    [fn, delay, ...dependencies] // Ensure fn and delay are included, and dependencies are spread
  );
};
