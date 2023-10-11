// source: https://github.com/antonioru/beautiful-react-hooks/blob/master/src/useThrottledCallback.ts

import { useCallback, useEffect, useRef, type DependencyList } from "react";
import { throttle } from "lodash";

interface ThrottleSettings {
  leading?: boolean | undefined;
  trailing?: boolean | undefined;
}

const defaultOptions: ThrottleSettings = {
  leading: false,
  trailing: true
};

/**
 * Accepts a function and returns a new throttled yet memoized version of that same function that waits the defined time
 * before allowing the next execution.
 * If time is not defined, its default value will be 250ms.
 */
export const useThrottledCallback = <TCallback extends (...args: any[]) => any>(
  fn: TCallback,
  dependencies: DependencyList,
  delay: number,
  options: ThrottleSettings = defaultOptions
) => {
  const throttled = useRef(throttle<TCallback>(fn, delay, options));

  useEffect(() => {
    throttled.current = throttle(fn, delay, options);
  }, [fn, delay, options]);

  useEffect(() => {
    return () => {
      throttled.current?.cancel();
    };
  }, []);

  // eslint-disable-next-line react-hooks/exhaustive-deps
  return useCallback(throttled.current, dependencies);
};
