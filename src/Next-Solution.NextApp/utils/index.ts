import { ClassValue, clsx } from "clsx";
import queryString, { StringifiableRecord, StringifyOptions, UrlObject } from "query-string";
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

export const buildUrl = (object: UrlObject, options?: StringifyOptions) => {
  const baseUrl = "https://example.com";
  const isRelativeUrl = (url: string): boolean => !/^([a-z][a-z\d+\-.]*:)?\/\//i.test(url);

  if (object.url && isRelativeUrl(object.url)) {
    // Prepend baseUrl for relative paths
    const result = queryString.stringifyUrl(
      { ...object, url: new URL(object.url, baseUrl).href },
      options
    );

    return result.replace(baseUrl, '');
  }

  return queryString.stringifyUrl(object, options);
};
