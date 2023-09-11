import { CSSProperties, MutableRefObject, useEffect, useState } from "react";

import { useIsomorphicLayoutEffect } from "./useIsomorphicLayoutEffect";

type UseLockedBodyOutput = [boolean, (locked: boolean) => void];

export function useLockedBody(
  initialLocked = false,
  initialOverflow?: CSSProperties["overflow"],
  ref?: MutableRefObject<HTMLElement> // Default to `___gatsby` to not introduce breaking change
): UseLockedBodyOutput {
  const [locked, setLocked] = useState(initialLocked);

  // Do the side effect before render
  useIsomorphicLayoutEffect(() => {
    if (!locked) {
      return;
    }

    const body = ref?.current ?? document.body;
    // Save initial body style
    const originalOverflow = body.style.overflow;
    const originalPaddingRight = body.style.paddingRight;

    // Lock body scroll
    body.style.overflow = "hidden";

    // Get the scrollBar width
    const root = ref?.current; // or root
    const scrollBarWidth = root ? root.offsetWidth - root.scrollWidth : 0;

    // Avoid width reflow
    if (scrollBarWidth) {
      body.style.paddingRight = `${scrollBarWidth}px`;
    }

    return () => {
      body.style.overflow = initialOverflow ?? originalOverflow;

      if (scrollBarWidth) {
        body.style.paddingRight = originalPaddingRight;
      }
    };
  }, [locked]);

  // Update state if initialValue changes
  useEffect(() => {
    if (locked !== initialLocked) {
      setLocked(initialLocked);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [initialLocked]);

  return [locked, setLocked];
}

export default useLockedBody;
