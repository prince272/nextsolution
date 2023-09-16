// source: https://dirask.com/posts/React-use-delayed-callback-with-state-reset-when-component-is-unmount-DKJe81

import { useEffect, useRef } from "react";

export function useDebounceCallback(delay: number = 100, cleaning: boolean = true) {
  const ref = useRef<number | undefined>();

  useEffect(() => {
    if (cleaning) {
      return () => {
        if (typeof window !== "undefined" && ref.current) {
          window.clearTimeout(ref.current);
        }
      };
    }
  }, [cleaning]);

  return (callback: () => void) => {
    if (typeof window !== "undefined") {
      if (ref.current) {
        window.clearTimeout(ref.current);
      }
      ref.current = window.setTimeout(callback, delay);
    }
  };
}
