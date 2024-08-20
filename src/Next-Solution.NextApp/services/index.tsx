import { prefix } from "@/utils";
import axios from "axios";
import { IdentityService } from "./identity-service";

const api = axios.create({
  baseURL: prefix("https://", process.env.NEXT_PUBLIC_SERVER_URL)!,
  withCredentials: true
});

if (api.defaults.baseURL) console.info(`Setting up API with server URL: ${api.defaults.baseURL}`);
else console.warn("No server URL found in environment variables");

const identityService = new IdentityService(api);

export { api, identityService };
