import { useCallback, useRef } from "react";
import type { DependencyList } from "react";
import { useTimeout } from "./use-timeout";

/**
 * Creates a throttled function that will invoke the input function at most once per
 * specified delay.
 *
 * @param fn a function that will be throttled
 * @param dependencies An array of dependencies that will trigger re-creation of the throttled function
 * @param delay The milliseconds delay before invoking the function again
 * @returns A throttled function
 */
export const useThrottledCallback = <TCallback extends (...args: any[]) => any>(
  fn: TCallback,
  dependencies: DependencyList,
  delay: number
): ((...args: Parameters<TCallback>) => void) => {
  const timeout = useTimeout();
  const lastCallTime = useRef<number | null>(null);

  return useCallback(
    (...args: Parameters<TCallback>) => {
      const now = Date.now();

      if (lastCallTime.current === null || now - lastCallTime.current >= delay) {
        lastCallTime.current = now;
        fn(...args);
      } else {
        timeout.set(
          () => {
            lastCallTime.current = Date.now();
            fn(...args);
          },
          delay - (now - lastCallTime.current)
        );
      }
    },
    [fn, delay, ...dependencies] // Ensure fn and delay are included, and dependencies are spread
  );
};
