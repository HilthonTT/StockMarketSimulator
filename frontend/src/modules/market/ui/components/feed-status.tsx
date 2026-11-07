"use client";

import { cn } from "@/lib/utils";
import { Hint } from "@/components/hint";

import { useFeedStore, type FeedStatus as Status } from "../../store";

const LABELS: Record<Status, { label: string; hint: string; dot: string }> = {
  idle: { label: "Offline", hint: "Not connected to the price feed", dot: "bg-muted-foreground" },
  connecting: { label: "Connecting", hint: "Connecting to the live price feed…", dot: "bg-amber-500 animate-pulse" },
  live: { label: "Live", hint: "Prices update in real time", dot: "bg-gain" },
  reconnecting: { label: "Reconnecting", hint: "Connection lost, retrying…", dot: "bg-amber-500 animate-pulse" },
  offline: { label: "Offline", hint: "Can't reach the price feed. Prices may be stale.", dot: "bg-loss" },
};

export const FeedStatus = ({ className }: { className?: string }) => {
  const status = useFeedStore((state) => state.status);
  const { label, hint, dot } = LABELS[status];

  return (
    <Hint label={hint}>
      <span
        className={cn(
          "inline-flex items-center gap-1.5 rounded-full border px-2.5 py-1 text-xs font-medium",
          className
        )}
        role="status"
        aria-live="polite"
      >
        <span className={cn("size-2 rounded-full", dot)} aria-hidden="true" />
        {label}
      </span>
    </Hint>
  );
};
