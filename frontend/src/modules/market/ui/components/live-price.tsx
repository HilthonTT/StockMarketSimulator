import type { Quote } from "../../store";

import { cn, formatCurrency } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";

interface LivePriceProps {
  quote: Quote | undefined;
  className?: string;
  skeletonClassName?: string;
}

/** Current price that briefly flashes green/red on every tick. */
export const LivePrice = ({ quote, className, skeletonClassName }: LivePriceProps) => {
  if (!quote) {
    return <Skeleton className={cn("h-5 w-16 inline-block align-middle", skeletonClassName)} />;
  }

  const direction =
    quote.price > quote.previousPrice ? "up" : quote.price < quote.previousPrice ? "down" : "flat";

  return (
    <span
      // Re-mounting on every tick restarts the flash animation without effects or timers.
      key={quote.updatedAt}
      className={cn(
        "rounded px-1 -mx-1 tabular-nums",
        direction === "up" && "animate-flash-up",
        direction === "down" && "animate-flash-down",
        className
      )}
    >
      {formatCurrency(quote.price)}
    </span>
  );
};
