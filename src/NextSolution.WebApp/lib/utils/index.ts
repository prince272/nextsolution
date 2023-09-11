import { LegacyRef, MutableRefObject, RefCallback } from "react";
import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function sleep(ms: number = 500) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

// react-merge-refs
// source: https://github.com/gregberge/react-merge-refs
export function mergeRefs<T = any>(refs: Array<MutableRefObject<T> | LegacyRef<T>>): RefCallback<T> {
  return (value) => {
    refs.forEach((ref) => {
      if (typeof ref === "function") {
        ref(value);
      } else if (ref != null) {
        (ref as MutableRefObject<T | null>).current = value;
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

export function parseJSON(text: string | any, defaultValue?: any): any {
  if (typeof text === "string") {
    try {
      return JSON.parse(text);
    } catch (error) {
      console.warn(`Invalid JSON String: ${error}`);
    }
  }
  return defaultValue;
}

export function stringifyJSON(value: any): string | null {
  try {
    return JSON.stringify(value);
  } catch (error) {
    console.warn(`Invalid JSON Value: ${error}`);
    return null;
  }
}

export function cleanObject(obj: Record<string, any>): Record<string, any> {
  let newObj: Record<string, any> = {};
  Object.keys(obj).forEach((key) => {
    if (obj[key] === Object(obj[key])) newObj[key] = cleanObject(obj[key]);
    else if (obj[key] !== undefined) newObj[key] = obj[key];
  });
  return newObj;
}

export function isEqualSearchParams(params1: URLSearchParams, params2: URLSearchParams): boolean {
  // Get all keys from both params1 and params2
  const keys1 = Array.from(params1.keys());
  const keys2 = Array.from(params2.keys());

  // Check if the number of keys is the same
  if (keys1.length !== keys2.length) {
    return false;
  }

  // Check if all keys in params1 exist in params2 and have the same values
  for (const key of keys1) {
    const value1 = params1.get(key);
    const value2 = params2.get(key);

    if (value1 !== value2) {
      return false;
    }
  }

  return true;
}
