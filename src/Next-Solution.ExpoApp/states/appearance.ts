import { StateCreator } from "zustand";

export interface AppearanceState {}

export interface AppearanceActions {}

export interface AppearanceSlice {
  appearance: AppearanceState & AppearanceActions;
}

export const createAppearanceSlice: StateCreator<AppearanceSlice, [], [], AppearanceSlice> = (
  set,
  get
) => ({
  appearance: {}
});
