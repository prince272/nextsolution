import { ApiConfig } from "./types";

export const apiConfig = {
  baseURL: process.env.SERVER_URL,
  withCredentials: true
} as ApiConfig;
