import { CreateAxiosDefaults } from "axios";

export type User = {
  id: string;
  userName: string;
  firstName: string;
  lastName: string;
  email?: string;
  emailRequired: boolean;
  emailConfirmed: boolean;
  phoneNumber?: string;
  phoneNumberRequired: boolean;
  phoneNumberConfirmed: boolean;
  avatarId?: string;
  avatarUrl: string;
  bio: string;
  active: boolean;
  activeAt: Date;
  tokenType: string;
  accessToken: string;
  refreshToken: string;
  [key: string]: any;
};

export interface ApiStore {
  get: (name: string) => any;
  set: (name: string, value: any) => void;
}

export interface ApiConfig extends CreateAxiosDefaults {
  store: ApiStore;
}
