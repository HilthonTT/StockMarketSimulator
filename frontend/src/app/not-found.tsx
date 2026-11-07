import Link from "next/link";
import { CompassIcon } from "lucide-react";

import { Button } from "@/components/ui/button";

export default function NotFound() {
  return (
    <main className="flex min-h-svh flex-col items-center justify-center gap-4 p-6 text-center">
      <div className="rounded-full bg-muted p-3">
        <CompassIcon className="size-6 text-muted-foreground" aria-hidden="true" />
      </div>
      <div>
        <h1 className="text-2xl font-bold">Page not found</h1>
        <p className="mt-1 text-muted-foreground">The page you&apos;re looking for doesn&apos;t exist.</p>
      </div>
      <Button asChild>
        <Link href="/">Back to dashboard</Link>
      </Button>
    </main>
  );
}
