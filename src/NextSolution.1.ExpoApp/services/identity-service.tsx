import { AxiosInstance } from "axios";
import { Succeeded, Result, Unauthorized, ValidationProblem } from "./results";
import {
  ChangeAccountForm,
  ChangePasswordForm,
  ConfirmAccountForm,
  CreateAccountForm,
  RefreshTokenForm,
  ResetPasswordForm,
  SignInForm,
  SignInProvider,
  SignOutForm,
  UserProfileModel,
  UserSessionModel
} from "./types";

export class IdentityService {
  constructor(private api: AxiosInstance) {}

  async createAccount<Form extends CreateAccountForm>(form: Form) {
    return await Result.handle<ValidationProblem<Form> | Succeeded>(this.api.post("/identity/create", form));
  }

  async confirmAccount<Form extends ConfirmAccountForm>(form: Form) {
    return await Result.handle<ValidationProblem<Form> | Succeeded>(this.api.post("/identity/confirm", form));
  }

  async changeAccount<Form extends ChangeAccountForm>(form: Form) {
    return await Result.handle<ValidationProblem<Form> | Unauthorized | Succeeded>(this.api.post("/identity/change", form));
  }

  async changePassword<Form extends ChangePasswordForm>(form: Form) {
    return await Result.handle<ValidationProblem<Form> | Unauthorized | Succeeded>(this.api.post("/identity/password/change", form));
  }

  async resetPassword<Form extends ResetPasswordForm>(form: Form) {
    return await Result.handle<ValidationProblem<Form> | Succeeded>(this.api.post("/identity/password/reset", form));
  }

  async signIn<Form extends SignInForm>(form: Form) {
    return await Result.handle<ValidationProblem<Form> | Succeeded<UserSessionModel>>(this.api.post("/identity/sign-in", form));
  }

  async signInWith(provider: SignInProvider) {
    return await Result.handle<ValidationProblem | Succeeded<UserSessionModel>>(this.api.post(`/identity/sign-in/${provider.toLowerCase()}`));
  }

  async refreshToken<Form extends RefreshTokenForm>(form: Form) {
    return await Result.handle<ValidationProblem<Form> | Succeeded<UserSessionModel>>(this.api.post("/identity/refresh-token", form));
  }

  async signOut<Form extends SignOutForm>(form: Form) {
    return await Result.handle<Unauthorized | Succeeded>(this.api.post("/identity/sign-out", form));
  }

  async getProfile() {
    return await Result.handle<Unauthorized | Succeeded<UserProfileModel>>(this.api.get("/identity/profile"));
  }
}
