import { create } from "zustand";

const defaultValues = {
  userId: "",
  profileImageId: "",
};

interface IProfilePicture {
  isOpen: boolean;
  onOpen: (userId: string, currentImageUrl: string) => void;
  onClose: () => void;
  initialValues: typeof defaultValues;
}

export const useProfilePictureModal = create<IProfilePicture>((set) => ({
  isOpen: false,
  onOpen: (userId, profileImageId) => {
    set({
      isOpen: true,
      initialValues: {
        userId,
        profileImageId,
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
