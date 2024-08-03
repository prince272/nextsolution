import { StateCreator } from "zustand";

export interface ApplicationState {
}

export interface ApplicationActions {
}

export interface ApplicationSlice {
    application: ApplicationState & ApplicationActions;
}

export const createApplicationSlice: StateCreator<ApplicationSlice, [], [], ApplicationSlice> = (set, get) => ({
    application: {
  }
});
