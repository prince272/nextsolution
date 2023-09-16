import { CreateAxiosDefaults } from "axios";

export type User = {
  id: string;
  userName: string;
  firstName: string;
  lastName: string;
  email?: string;
  emailFirst: boolean;
  emailConfirmed: boolean;
  phoneNumber?: string;
  phoneNumberFirst: boolean;
  phoneNumberConfirmed: boolean;
  avatarId?: string;
  avatarUrl: string;
  active: boolean;
  activeAt: Date;
  tokenType: string;
  accessToken: string;
  refreshToken: string;
  [key: string]: any;
};

export interface ApiState {
  user?: User | null | undefined;
}

export interface ApiStore {
  get: (name: string) => any;
  set: (name: string, value: any) => void;
}

export interface ApiConfig extends CreateAxiosDefaults {
  store: ApiStore;
}
