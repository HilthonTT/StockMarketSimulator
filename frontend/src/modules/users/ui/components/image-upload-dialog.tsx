"use client";

import { useRef, useState } from "react";
import { ImageIcon, LoaderIcon } from "lucide-react";
import { toast } from "sonner";

import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

const ACCEPTED_TYPES = ["image/png", "image/jpeg"];
const MAX_SIZE_BYTES = 5 * 1024 * 1024;

interface ImageUploadDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description: string;
  currentImageUrl?: string;
  shape: "circle" | "banner";
  onUpload: (file: File) => Promise<void>;
}

export const ImageUploadDialog = (props: ImageUploadDialogProps) => (
  <Dialog open={props.open} onOpenChange={props.onOpenChange}>
    <DialogContent className="sm:max-w-lg">
      {/* Mounted only while open, so every opening starts with a clean selection. */}
      {props.open && <ImageUploadForm {...props} />}
    </DialogContent>
  </Dialog>
);

const ImageUploadForm = ({
  onOpenChange,
  title,
  description,
  currentImageUrl,
  shape,
  onUpload,
}: ImageUploadDialogProps) => {
  const inputRef = useRef<HTMLInputElement>(null);
  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);

  const handleFile = (selected: File | undefined) => {
    if (!selected) {
      return;
    }

    if (!ACCEPTED_TYPES.includes(selected.type)) {
      toast.error("Please choose a PNG or JPG image.");
      return;
    }

    if (selected.size > MAX_SIZE_BYTES) {
      toast.error("That image is larger than 5 MB.");
      return;
    }

    setFile(selected);
    setPreview(URL.createObjectURL(selected));
  };

  const handleSave = async () => {
    if (!file) {
      return;
    }

    setIsUploading(true);
    try {
      await onUpload(file);
      onOpenChange(false);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Upload failed. Please try again.");
    } finally {
      setIsUploading(false);
    }
  };

  const shown = preview ?? currentImageUrl;

  return (
    <>
      <DialogHeader>
        <DialogTitle>{title}</DialogTitle>
        <DialogDescription>{description}</DialogDescription>
      </DialogHeader>

      <input
        ref={inputRef}
        type="file"
        accept={ACCEPTED_TYPES.join(",")}
        className="hidden"
        onChange={(event) => handleFile(event.target.files?.[0])}
      />

      <button
        type="button"
        onClick={() => inputRef.current?.click()}
        onDragOver={(event) => event.preventDefault()}
        onDrop={(event) => {
          event.preventDefault();
          handleFile(event.dataTransfer.files?.[0]);
        }}
        disabled={isUploading}
        className="flex min-h-56 w-full cursor-pointer flex-col items-center justify-center gap-3 rounded-xl border-2 border-dashed p-4 transition-colors hover:bg-muted/50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
      >
        {shown ? (
          // eslint-disable-next-line @next/next/no-img-element -- blob/remote preview, no optimisation needed
          <img
            src={shown}
            alt="Selected image preview"
            className={cn("object-cover", shape === "circle" ? "size-40 rounded-full" : "h-40 w-full rounded-lg")}
          />
        ) : (
          <ImageIcon className="size-10 text-muted-foreground" aria-hidden="true" />
        )}
        <span className="text-sm">
          <span className="font-semibold underline-offset-4 hover:underline">
            {shown ? "Choose a different image" : "Choose an image"}
          </span>{" "}
          <span className="text-muted-foreground">or drag it here</span>
        </span>
        <span className="text-xs text-muted-foreground">PNG or JPG, up to 5 MB</span>
      </button>

      <DialogFooter>
        <Button variant="outline" onClick={() => onOpenChange(false)} disabled={isUploading}>
          Cancel
        </Button>
        <Button onClick={handleSave} disabled={!file || isUploading}>
          {isUploading && <LoaderIcon className="animate-spin" />}
          Save
        </Button>
      </DialogFooter>
    </>
  );
};
