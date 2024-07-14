import { AxiosInstance } from "axios";

import { Ok, Result, ValidationProblem } from "./results";
import { ChangeAccountForm, ChangePasswordForm, ConfirmAccountForm, CreateAccountForm, RefreshTokenForm, ResetPasswordForm, SignInForm, SignOutForm, UserProfileModel, UserSessionModel } from "./types";

export class IdentityService {
  constructor(private api: AxiosInstance) {}

  async createAccountAsync<Form extends CreateAccountForm>(form: Form) {
    return await Result.handle<ValidationProblem<Form> | Ok>(this.api.post("/identity/create", form));
  }

  async confirmAccountAsync<Form extends ConfirmAccountForm>(form: Form) { 
    return await Result.handle<ValidationProblem<Form> | Ok>(this.api.post("/identity/confirm", form));
  }

  async changeAccountAsync<Form extends ChangeAccountForm>(form: Form) {
    return await Result.handle<ValidationProblem<Form> | Ok>(this.api.post("/identity/change", form));
  }

  async changePasswordAsync<Form extends ChangePasswordForm>(form: Form) {
    return await Result.handle<ValidationProblem<Form> | Ok>(this.api.post("/identity/password/change", form));
  }

  async resetPasswordAsync<Form extends ResetPasswordForm>(form: Form) {
    return await Result.handle<ValidationProblem<Form> | Ok>(this.api.post("/identity/password/reset", form));
  }

  async signInAsync<Form extends SignInForm>(form: Form) {
    return await Result.handle<ValidationProblem<Form> | Ok<UserSessionModel>>(this.api.post("/identity/signin", form));
  }

  async refreshTokenAsync<Form extends RefreshTokenForm>(form: Form) {
    return await Result.handle<ValidationProblem<Form> | Ok<UserSessionModel>>(this.api.post("/identity/refresh-token", form));
  }

  async signOutAsync<Form extends SignOutForm>(form: Form) {
    return await Result.handle<Ok>(this.api.post("/identity/signout", form));
  }

  async getProfileAsync() {
    return await Result.handle<Ok<UserProfileModel>>(this.api.get("/identity/profile"));
  }
}
