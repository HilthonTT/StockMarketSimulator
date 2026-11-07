import { ArrowDownRightIcon, ArrowUpRightIcon, MinusIcon } from "lucide-react";

import { cn, formatPercent, formatSignedCurrency } from "@/lib/utils";

interface PriceChangeProps {
  /** Absolute change in dollars. */
  change: number;
  /** Change as a ratio (0.01 = 1%). */
  ratio?: number;
  variant?: "text" | "badge";
  showIcon?: boolean;
  className?: string;
}

/** Green/red signed change, e.g. `▲ +$1.20 (+0.85%)`. */
export const PriceChange = ({
  change,
  ratio,
  variant = "text",
  showIcon = true,
  className,
}: PriceChangeProps) => {
  // Hide float noise like -0.000001.
  const rounded = Math.round(change * 100) / 100;
  const direction = rounded > 0 ? "up" : rounded < 0 ? "down" : "flat";

  const Icon =
    direction === "up" ? ArrowUpRightIcon : direction === "down" ? ArrowDownRightIcon : MinusIcon;

  return (
    <span
      className={cn(
        "inline-flex items-center gap-0.5 font-medium tabular-nums whitespace-nowrap",
        direction === "up" && "text-gain",
        direction === "down" && "text-loss",
        direction === "flat" && "text-muted-foreground",
        variant === "badge" && "rounded-md px-1.5 py-0.5 text-xs",
        variant === "badge" && direction === "up" && "bg-gain/10",
        variant === "badge" && direction === "down" && "bg-loss/10",
        variant === "badge" && direction === "flat" && "bg-muted",
        className
      )}
    >
      {showIcon && <Icon className="size-3.5 shrink-0" aria-hidden="true" />}
      {formatSignedCurrency(rounded)}
      {ratio !== undefined && <span className="opacity-80">({formatPercent(ratio)})</span>}
    </span>
  );
};

/** Percent-only variant for dense tables. */
export const PercentChange = ({ ratio, className }: { ratio: number; className?: string }) => {
  const rounded = Math.round(ratio * 10000) / 10000;

  return (
    <span
      className={cn(
        "font-medium tabular-nums whitespace-nowrap",
        rounded > 0 && "text-gain",
        rounded < 0 && "text-loss",
        rounded === 0 && "text-muted-foreground",
        className
      )}
    >
      {formatPercent(rounded)}
    </span>
  );
};
