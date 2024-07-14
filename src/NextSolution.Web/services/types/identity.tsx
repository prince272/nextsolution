export type CreateAccountForm = {
  firstName: string;
  lastName?: string;
  username: string;
  usernameType?: "email" | "phoneNumber";
  password: string;
};

export type ChangeAccountForm = {
  newUsernameType?: "email" | "phoneNumber";
  newUsername: string;
  code: string;
  process: "sendCode" | "verifyCode";
};
