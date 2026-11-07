import Link from "next/link";
import { formatDistanceToNow } from "date-fns";

import { cn, formatCurrency, formatNumber, parseUtc } from "@/lib/utils";

import type { TransactionResponse } from "../../types";
import { isBuy, SideBadge } from "./side-badge";

interface TransactionListProps {
  transactions: TransactionResponse[];
  className?: string;
}

/** Compact activity feed: "BUY 3 AAPL @ $190.12 · 2 minutes ago". */
export const TransactionList = ({ transactions, className }: TransactionListProps) => {
  return (
    <ul className={cn("divide-y", className)}>
      {transactions.map((transaction) => {
        const total = transaction.amount * transaction.quantity;

        return (
          <li key={transaction.id} className="flex items-center gap-3 py-3">
            <SideBadge type={transaction.type} />
            <div className="min-w-0 flex-1">
              <p className="truncate text-sm">
                {formatNumber(transaction.quantity)}{" "}
                <Link
                  href={`/stocks/${transaction.ticker.toUpperCase()}`}
                  className="font-semibold hover:underline"
                >
                  {transaction.ticker.toUpperCase()}
                </Link>{" "}
                <span className="text-muted-foreground">@ {formatCurrency(transaction.amount)}</span>
              </p>
              <p className="text-xs text-muted-foreground">
                {formatDistanceToNow(parseUtc(transaction.createdOnUtc), { addSuffix: true })}
              </p>
            </div>
            <span
              className={cn(
                "text-sm font-medium tabular-nums",
                isBuy(transaction.type) ? "text-foreground" : "text-gain"
              )}
            >
              {isBuy(transaction.type) ? "-" : "+"}
              {formatCurrency(total)}
            </span>
          </li>
        );
      })}
    </ul>
  );
};
