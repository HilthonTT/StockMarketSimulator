import { create } from "zustand";

const defaultValues = {
  ticker: "",
};

interface ITransactionModal {
  isOpen: boolean;
  onOpen: (ticker: string) => void;
  onClose: () => void;
  initialValues: typeof defaultValues;
}

export const useTransactionModal = create<ITransactionModal>((set) => ({
  isOpen: false,
  onOpen: (ticker) => {
    set({ isOpen: true, initialValues: { ticker } });
  },
  onClose: () => {
    set({
      isOpen: false,
      initialValues: defaultValues,
    });
  },
  initialValues: defaultValues,
}));
