import { useMemo, useState } from "react";

import useEventListener from "./useEventListener";

/**
 * Reactive scroll values for a react ref or a dom node
 *
 * @param target - dom node or react ref
 * @param callback - callback to run on scroll
 *
 * @see https://react-hooks-library.vercel.app/core/useScroll
 */
export function useScroll<T extends Element | HTMLElement | Window>(target: T, callback?: (coords: { scrollX: number; scrollY: number }) => void) {
  const [scrollValues, setScrollValues] = useState(useMemo(() => getPositions(target), [target]));

  useEventListener(
    target,
    "scroll",
    () => {
      const newScrollValues = getPositions(target);
      if (!newScrollValues) return;
      setScrollValues(newScrollValues);
      callback?.(newScrollValues);
    },
    {
      capture: false,
      passive: true
    }
  );

  return { ...scrollValues };
}
function getPositions<T extends Element | HTMLElement | Window>(target: T) {
  const round = (num: number) => Math.round(num * 1e2) / 1e2;

  if (target instanceof Window) {
    // Get the current scroll position in both X and Y directions
    const scrollX = window.scrollX || window.pageXOffset || document.documentElement.scrollLeft;
    const scrollY = window.scrollY || window.pageYOffset || document.documentElement.scrollTop;

    // Get the maximum scrollable width and height of the page
    const scrollWidth = Math.max(
      document.body.scrollWidth,
      document.body.offsetWidth,
      document.documentElement.clientWidth,
      document.documentElement.scrollWidth,
      document.documentElement.offsetWidth
    );

    const scrollHeight = Math.max(
      document.body.scrollHeight,
      document.body.offsetHeight,
      document.documentElement.clientHeight,
      document.documentElement.scrollHeight,
      document.documentElement.offsetHeight
    );

    return {
      scrollX: round(scrollX / (scrollWidth - window.innerWidth)),
      scrollY: round(scrollY / (scrollHeight - window.innerHeight))
    };
  } else {
    return {
      scrollX: round(target.scrollLeft / (target.scrollWidth - target.clientWidth)),
      scrollY: round(target.scrollTop / (target.scrollHeight - target.clientHeight))
    };
  }
}
