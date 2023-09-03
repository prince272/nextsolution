import { CreateAxiosDefaults } from "axios";

export type User = {
  id: string;
  userName: string;
  firstName: string;
  lastName: string;
  email?: string;
  emailConfirmed: boolean;
  phoneNumber?: string;
  phoneNumberConfirmed: boolean;
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
