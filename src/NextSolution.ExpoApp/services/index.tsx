import axios from "axios";

import { IdentityService } from "./identity-service";

const api = axios.create({
  baseURL: process.env.SERVER_URL,
  withCredentials: true
});

const identityService = new IdentityService(api);

export { api, identityService };
