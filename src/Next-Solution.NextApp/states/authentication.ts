import { StateCreator } from "zustand";
import { UserSessionModel } from "@/services/types";

export interface AuthenticationState {
  user?: UserSessionModel | null | undefined;
}

export interface AuthenticationActions {
  setUser: (user: UserSessionModel) => void;
  clearUser: () => void;
}

export interface AuthenticationSlice {
  authentication: AuthenticationState & AuthenticationActions;
}

export const createAuthenticationSlice: StateCreator<
  AuthenticationSlice,
  [],
  [],
  AuthenticationSlice
> = (set, get) => ({
  authentication: {
    user: null,
    setUser: (user) => set((state) => ({ authentication: { ...state.authentication, user } })),
    clearUser: () => set((state) => ({ authentication: { ...state.authentication, user: null } }))
  }
});
