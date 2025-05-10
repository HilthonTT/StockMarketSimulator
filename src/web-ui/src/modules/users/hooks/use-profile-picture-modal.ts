import { create } from "zustand";

const defaultValues = {
  userId: "",
  currentImageUrl: "",
};

interface IProfilePicture {
  isOpen: boolean;
  onOpen: (userId: string, currentImageUrl: string) => void;
  onClose: () => void;
  initialValues: typeof defaultValues;
}

export const useProfilePictureModal = create<IProfilePicture>((set) => ({
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
