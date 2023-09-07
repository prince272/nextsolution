import { CSSProperties, MutableRefObject, useEffect, useState } from "react";

import { useIsomorphicLayoutEffect } from "./useIsomorphicLayoutEffect";

type UseLockedBodyOutput = [boolean, (locked: boolean) => void];

export function useLockedBody(initialOverflow?: CSSProperties["overflow"], ref?: MutableRefObject<HTMLElement>): UseLockedBodyOutput {
  const initialLocked = initialOverflow == "hidden";
  const [locked, setLocked] = useState(initialLocked);

  // Do the side effect before render
  useIsomorphicLayoutEffect(() => {
    if (!locked) {
      return;
    }

    const element = ref?.current ?? document.body;

    // Save initial body style
    const originalOverflow = element.style.overflow;
    const originalPaddingRight = element.style.paddingRight;

    // Lock body scroll
    element.style.overflow = "hidden";

    // Get the scrollBar width
    const scrollBarWidth = element.offsetWidth - element.scrollWidth;

    // Avoid width reflow
    if (scrollBarWidth) {
      element.style.paddingRight = `${scrollBarWidth}px`;
    }

    return () => {
      element.style.overflow = initialOverflow ?? originalOverflow;

      if (scrollBarWidth) {
        element.style.paddingRight = initialOverflow ?? originalPaddingRight;
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
