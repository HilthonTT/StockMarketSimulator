"use client";

import { CameraIcon } from "lucide-react";

import type { UserResponse } from "@/modules/auth/types";

import { ProfileBannerModal } from "./profile-banner-modal";

import { useProfileBannerModal } from "../../hooks/use-profile-banner-modal";
import { getImageUrl } from "@/lib/utils";

interface ProfileBannerProps {
  user: UserResponse;
}

export const ProfileBanner = ({ user }: ProfileBannerProps) => {
  const { onOpen } = useProfileBannerModal();

  const imageUrl = user.bannerImageId
    ? getImageUrl(user.bannerImageId)
    : "banner.jpg";

  return (
    <>
      <ProfileBannerModal />

      <div className="relative group" onClick={() => onOpen(user.id, "")}>
        <div
          className="w-full h-[15vh] md:h-[25vh] rounded-xl cursor-pointer hover:opacity-80 transition-opacity duration-300"
          style={{
            backgroundImage: `url(${imageUrl})`,
            backgroundSize: "cover",
            backgroundPosition: "center",
            backgroundRepeat: "no-repeat",
          }}
        />

        <div className="group-hover:opacity-100 opacity-0 absolute bottom-2 right-3 cursor-pointer transition-opacity duration-300">
          <div className="relative rounded-full bg-white p-2">
            <CameraIcon className="size-6" />
          </div>
        </div>
      </div>
    </>
  );
};
