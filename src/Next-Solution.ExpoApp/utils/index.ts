import { ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

export const cn = (...inputs: ClassValue[]) => {
  return twMerge(clsx(inputs));
};

export const sleep = (ms: number) => {
  return new Promise((resolve) => setTimeout(resolve, ms));
};

export const prefix = (prefix: string, s: string | null | undefined) => {
  if (s == null) return s; // Return s if it is null or undefined
  return s.startsWith(prefix) ? s : prefix + s;
};

export const suffix = (suffix: string, s: string | null | undefined) => {
  if (s == null) return s; // Return s if it is null or undefined
  return s.endsWith(suffix) ? s : s + suffix;
};
