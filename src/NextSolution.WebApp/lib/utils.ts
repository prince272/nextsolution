import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

// react-merge-refs
// source: https://github.com/gregberge/react-merge-refs
export function mergeRefs<T = any>(refs: Array<React.MutableRefObject<T> | React.LegacyRef<T>>): React.RefCallback<T> {
  return (value) => {
    refs.forEach((ref) => {
      if (typeof ref === "function") {
        ref(value);
      } else if (ref != null) {
        (ref as React.MutableRefObject<T | null>).current = value;
      }
    });
  };
}

export function isAbsoluteUrl(url: string): boolean {
  try {
    // Try to create a URL object using the provided URL string
    new URL(url);
    return true; // If successful, the URL is absolute
  } catch (error) {
    return false; // If an error occurs, the URL is relative
  }
}