export type CreateAccountForm = {
  firstName: string;
  lastName?: string;
  username: string;
  usernameType?: "email" | "phoneNumber";
  password: string;
};

export type ConfirmAccountForm = {
  username: string;
  usernameType?: "email" | "phoneNumber";
  code?: string;
  process: "sendCode" | "verifyCode";
};

export type ChangeAccountForm = {
  newUsernameType?: "email" | "phoneNumber";
  newUsername: string;
  code?: string;
  process: "sendCode" | "verifyCode";
};

export type ChangePasswordForm = {
  oldPassword?: string;
  newPassword: string;
  confirmPassword: string;
};

export type SendResetPasswordCodeForm = {
  username: string;
  usernameType?: "email" | "phoneNumber";
};

export type ResetPasswordForm = {
  username: string;
  usernameType?: "email" | "phoneNumber";
  code: string;
  newPassword: string;
  confirmPassword: string;
};

export type SignInForm = {
  username: string;
  usernameType?: "email" | "phoneNumber";
  password: string;
};

export type SignInWithProvider = "Google" | "Facebook";

export type RefreshTokenForm = {
  refreshToken: string;
};

export type SignOutForm = {
  refreshToken: string;
};

export type UserProfileModel = {
  id: string;
  firstName: string;
  lastName?: string;
  userName: string;
  email?: string;
  emailConfirmed: boolean;
  phoneNumber?: string;
  phoneNumberConfirmed: boolean;
  passwordConfigured: boolean;
  roles: string[];
};

export type UserSessionModel = UserProfileModel & {
  tokenType: string;
  accessToken: string;
  accessTokenExpiresAt: Date;
  refreshToken: string;
  refreshTokenExpiresAt: Date;
};
