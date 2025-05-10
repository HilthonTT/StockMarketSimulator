import { create } from "zustand";

const defaultValues = {
  userId: "",
  bannerImageId: "",
};

interface IProfileBanner {
  isOpen: boolean;
  onOpen: (userId: string, currentImageUrl: string) => void;
  onClose: () => void;
  initialValues: typeof defaultValues;
}

export const useProfileBannerModal = create<IProfileBanner>((set) => ({
  isOpen: false,
  onOpen: (userId, bannerImageId) => {
    set({
      isOpen: true,
      initialValues: {
        userId,
        bannerImageId,
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
