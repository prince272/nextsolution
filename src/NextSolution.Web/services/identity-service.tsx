import { AxiosInstance } from "axios";

import { Ok, Result, ValidationProblem } from "./results";
import { CreateAccountForm } from "./types";

export class IdentityService {
  constructor(private api: AxiosInstance) {}

  async createAccountAsync<Form extends CreateAccountForm>(form: Form) {
    return await Result.handle<ValidationProblem<Form> | Ok>(this.api.post("/identity/create", form));
  }
}
