"use client";

import Image from "next/image";
import { useRef, useState } from "react";
import { ImageIcon } from "lucide-react";

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

import { useProfilePictureModal } from "../../hooks/use-profile-picture-modal";

export const ProfilePictureModal = () => {
  const { isOpen, onClose } = useProfilePictureModal();

  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedImage, setSelectedImage] = useState<string | null>(null);

  const handleClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];

    if (!file) {
      return;
    }

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

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogContent className="max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="text-3xl text-center">
            Change your profile picture
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
            <div className="relative size-56 rounded-full overflow-hidden flex items-center justify-center">
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
            disabled={!selectedImage?.trim()}
          >
            Save
          </Button>
          {selectedImage && (
            <Button
              onClick={() => setSelectedImage(null)}
              variant="elevated"
              className="w-full"
            >
              Remove
            </Button>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
};
