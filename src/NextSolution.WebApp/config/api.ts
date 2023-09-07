import { ApiConfig } from "@/lib/api/types";

export const apiConfig = {
  baseURL: process.env.SERVER_URL,
  withCredentials: true
} as ApiConfig;
