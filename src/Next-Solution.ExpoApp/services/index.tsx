import axios from "axios";
import { IdentityService } from "./identity-service";

const api = axios.create({
  baseURL: process.env.EXPO_PUBLIC_SERVER_URL,
  withCredentials: true
});

console.info(`Setting up API with server URL: ${api.defaults.baseURL}`);

const identityService = new IdentityService(api);

export { api, identityService };
