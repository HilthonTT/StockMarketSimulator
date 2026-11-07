"use client";

import { useState } from "react";
import { format } from "date-fns";
import { CameraIcon } from "lucide-react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

import type { UserResponse } from "@/modules/auth/types";

import { useTRPC } from "@/trpc/client";
import { fetchFromApi } from "@/lib/api";
import { getImageUrl, parseUtc } from "@/lib/utils";
import { UserAvatar } from "@/components/user-avatar";
import { Card } from "@/components/ui/card";

import { ImageUploadDialog } from "./image-upload-dialog";

interface ProfileHeaderProps {
  user: UserResponse;
}

export const ProfileHeader = ({ user }: ProfileHeaderProps) => {
  const trpc = useTRPC();
  const queryClient = useQueryClient();
  const { data: jwt } = useQuery(trpc.auth.getJwt.queryOptions());

  const [dialog, setDialog] = useState<"avatar" | "banner" | null>(null);

  const bannerUrl = user.bannerImageId ? getImageUrl(user.bannerImageId) : "/banner.jpg";
  const avatarUrl = user.profileImageId ? getImageUrl(user.profileImageId) : "/profile.png";

  const upload = (kind: "profile-image" | "banner-image") => async (file: File) => {
    if (!jwt) {
      throw new Error("Your session has expired. Please log in again.");
    }

    await fetchFromApi({
      accessToken: jwt,
      path: `/api/v1/users/${user.id}/${kind}`,
      method: "PATCH",
      body: file,
      throwOnNotFound: true,
    });

    toast.success(kind === "profile-image" ? "Profile picture updated" : "Banner updated");
    await queryClient.invalidateQueries(trpc.users.getCurrent.queryFilter());
  };

  return (
    <Card className="gap-0 overflow-hidden py-0">
      <button
        type="button"
        onClick={() => setDialog("banner")}
        className="group relative h-32 w-full cursor-pointer bg-muted bg-cover bg-center sm:h-44"
        style={{ backgroundImage: `url(${bannerUrl})` }}
        aria-label="Change banner image"
      >
        <span className="absolute bottom-3 right-3 inline-flex items-center gap-1.5 rounded-full bg-background/90 px-3 py-1.5 text-xs font-medium opacity-0 shadow transition-opacity group-hover:opacity-100 group-focus-visible:opacity-100">
          <CameraIcon className="size-3.5" /> Change banner
        </span>
      </button>

      <div className="flex flex-col gap-4 px-6 pb-6 sm:flex-row sm:items-end">
        <button
          type="button"
          onClick={() => setDialog("avatar")}
          className="group relative -mt-12 w-fit cursor-pointer rounded-full ring-4 ring-card"
          aria-label="Change profile picture"
        >
          <UserAvatar size="xl" imageUrl={avatarUrl} name={user.username} className="size-24 bg-muted" />
          <span className="absolute inset-0 flex items-center justify-center rounded-full bg-black/40 opacity-0 transition-opacity group-hover:opacity-100 group-focus-visible:opacity-100">
            <CameraIcon className="size-6 text-white" />
          </span>
        </button>
        <div className="min-w-0">
          <h1 className="truncate text-2xl font-bold">{user.username}</h1>
          <p className="truncate text-sm text-muted-foreground">
            {user.email} · Trading since {format(parseUtc(user.createdOnUtc), "MMMM yyyy")}
          </p>
        </div>
      </div>

      <ImageUploadDialog
        open={dialog === "avatar"}
        onOpenChange={(open) => setDialog(open ? "avatar" : null)}
        title="Profile picture"
        description="Shown in the sidebar and on your profile."
        currentImageUrl={avatarUrl}
        shape="circle"
        onUpload={upload("profile-image")}
      />
      <ImageUploadDialog
        open={dialog === "banner"}
        onOpenChange={(open) => setDialog(open ? "banner" : null)}
        title="Banner image"
        description="A wide image works best."
        currentImageUrl={bannerUrl}
        shape="banner"
        onUpload={upload("banner-image")}
      />
    </Card>
  );
};
