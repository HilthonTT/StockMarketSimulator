import { create } from "zustand";

interface CommandSearchState {
  isOpen: boolean;
  setOpen: (isOpen: boolean) => void;
}

export const useCommandSearch = create<CommandSearchState>((set) => ({
  isOpen: false,
  setOpen: (isOpen) => set({ isOpen }),
}));
