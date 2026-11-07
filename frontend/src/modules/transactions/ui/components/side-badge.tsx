import { cn } from "@/lib/utils";

import { TransactionType } from "../../types";

/** Expense = money out = a buy; Income = money in = a sell. */
export const isBuy = (type: TransactionType) => type === TransactionType.Expense;

export const SideBadge = ({ type, className }: { type: TransactionType; className?: string }) => {
  const buy = isBuy(type);

  return (
    <span
      className={cn(
        "inline-flex w-12 justify-center rounded-md px-1.5 py-0.5 text-xs font-semibold uppercase",
        buy ? "bg-gain/10 text-gain" : "bg-loss/10 text-loss",
        className
      )}
    >
      {buy ? "Buy" : "Sell"}
    </span>
  );
};
