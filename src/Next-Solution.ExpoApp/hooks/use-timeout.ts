import { MutableRefObject, useEffect, useMemo, useRef } from "react";

const MAX_DELAY_MS = 2 ** 31 - 1;

const setChainedTimeout = (
  handleRef: MutableRefObject<any>,
  fn: () => void,
  timeoutAtMs: number
) => {
  const delayMs = timeoutAtMs - Date.now();

  handleRef.current =
    delayMs <= MAX_DELAY_MS
      ? setTimeout(fn, delayMs)
      : setTimeout(() => setChainedTimeout(handleRef, fn, timeoutAtMs), MAX_DELAY_MS);
};

export const useTimeout = () => {
  const handleRef = useRef<any>();

  useEffect(() => {
    return () => {
      clearTimeout(handleRef.current);
    };
  }, []);

  return useMemo(() => {
    const clear = () => clearTimeout(handleRef.current);

    const set = (fn: () => void, delayMs = 0): void => {
      clear();

      if (delayMs <= MAX_DELAY_MS) {
        handleRef.current = setTimeout(fn, delayMs);
      } else {
        setChainedTimeout(handleRef, fn, Date.now() + delayMs);
      }
    };

    return {
      set,
      clear
    };
  }, []);
};
