"use client";

import { AlertTriangleIcon, RotateCcwIcon } from "lucide-react";

import { Button } from "@/components/ui/button";

export default function Error({ error, reset }: { error: Error & { digest?: string }; reset: () => void }) {
  return (
    <div role="alert" className="flex flex-col items-center justify-center gap-4 py-20 text-center">
      <AlertTriangleIcon className="size-8 text-loss" aria-hidden="true" />
      <div>
        <h1 className="text-xl font-bold">Something went wrong</h1>
        <p className="mt-1 text-muted-foreground">{error.message || "An unexpected error occurred."}</p>
      </div>
      <Button variant="outline" onClick={reset}>
        <RotateCcwIcon /> Try again
      </Button>
    </div>
  );
}
