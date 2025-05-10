"use client";

import Image from "next/image";
import { useEffect, useRef, useState } from "react";
import { toast } from "sonner";
import { ImageIcon } from "lucide-react";
import { useQueryClient, useSuspenseQuery } from "@tanstack/react-query";

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { cn, getImageUrl } from "@/lib/utils";
import { useTRPC } from "@/trpc/client";
import { fetchFromApi } from "@/lib/api";

import { useProfileBannerModal } from "../../hooks/use-profile-banner-modal";

export const ProfileBannerModal = () => {
  const trpc = useTRPC();
  const queryClient = useQueryClient();

  const { isOpen, onClose, initialValues } = useProfileBannerModal();

  const { data: jwt } = useSuspenseQuery(trpc.auth.getJwt.queryOptions());

  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedImage, setSelectedImage] = useState<string | null>();
  const [imageFile, setImageFile] = useState<File | null>(null);

  const [isPending, setIsPending] = useState(false);

  const handleClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];

    if (!file) {
      return;
    }

    setImageFile(file);

    const reader = new FileReader();
    reader.onloadend = () => {
      setSelectedImage(reader.result as string);
    };
    reader.readAsDataURL(file);
  };

  const handleClose = () => {
    onClose();

    setSelectedImage(null);
  };

  const handleUpdate = async () => {
    if (!imageFile || !jwt) {
      return;
    }

    setIsPending(true);

    try {
      await fetchFromApi({
        accessToken: jwt,
        path: `/api/v1/users/${initialValues.userId}/banner-image`,
        method: "PATCH",
        body: imageFile,
      });

      toast.success("Banner picture updated!");

      await queryClient.invalidateQueries(trpc.users.getCurrent.queryOptions());

      handleClose();
    } catch (error) {
      console.error(error);
      toast.error("Something went wrong!");
    } finally {
      setIsPending(false);
    }
  };

  useEffect(() => {
    const url = getImageUrl(initialValues.bannerImageId);

    setSelectedImage(url);
  }, [initialValues.bannerImageId]);

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogContent className="max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="text-3xl text-center">
            Change your banner
          </DialogTitle>
        </DialogHeader>

        <input
          type="file"
          accept="image/png, image/jpeg, image/jpg"
          ref={fileInputRef}
          onChange={handleFileChange}
          className="hidden"
        />

        <button
          onClick={handleClick}
          tabIndex={0}
          className={cn(
            "size-full h-[300px] p-4 border-2 border-dashed rounded-xl cursor-pointer hover:opacity-80 transition-opacity flex items-center justify-center",
            selectedImage?.trim() && "hover:opacity-100 pointer-events-none"
          )}
        >
          {selectedImage ? (
            <div className="relative w-full h-48 rounded-xl overflow-hidden">
              <Image
                src={selectedImage}
                alt="Selected"
                fill
                className="object-cover"
              />
            </div>
          ) : (
            <div className="flex items-center justify-center flex-col">
              <ImageIcon className="size-18 mb-4" />
              <p className="font-semibold">
                Click here to{" "}
                <span className="font-bold text-blue-500">browse</span> your
                images
              </p>
              <p className="text-muted-foreground text-xs mt-1">
                Supports PNG, JPG, JPEG
              </p>
            </div>
          )}
        </button>
        <div className="w-full flex flex-col gap-y-3">
          <Button
            variant="elevated"
            className="w-full"
            disabled={!selectedImage?.trim() || isPending}
            onClick={handleUpdate}
          >
            Save
          </Button>
          {selectedImage && (
            <Button
              onClick={() => setSelectedImage(null)}
              variant="elevated"
              className="w-full"
              disabled={isPending}
            >
              Remove
            </Button>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
};
