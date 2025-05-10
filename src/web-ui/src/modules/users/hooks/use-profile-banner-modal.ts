import { create } from "zustand";

const defaultValues = {
  userId: "",
  currentImageUrl: "",
};

interface IProfileBanner {
  isOpen: boolean;
  onOpen: (userId: string, currentImageUrl: string) => void;
  onClose: () => void;
  initialValues: typeof defaultValues;
}

export const useProfileBannerModal = create<IProfileBanner>((set) => ({
  isOpen: false,
  onOpen: (userId, currentImageUrl) => {
    set({
      isOpen: true,
      initialValues: {
        userId,
        currentImageUrl,
      },
    });
  },
  onClose: () => {
    set({
      isOpen: false,
      initialValues: defaultValues,
    });
  },
  initialValues: defaultValues,
}));
