"use client";

import { useState } from "react";
import { InfoIcon, LoaderIcon, MinusIcon, PlusIcon } from "lucide-react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { toast } from "sonner";

import { useLiveQuote } from "@/modules/market/hooks/use-live-quote";
import { LivePrice } from "@/modules/market/ui/components/live-price";

import { useTRPC } from "@/trpc/client";
import { cn, formatCurrency, formatNumber } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";

import type { TradeSide } from "../../hooks/use-trade-dialog";
import { useInvalidateAccount } from "../../hooks/use-invalidate-account";

interface TradeTicketProps {
  ticker: string;
  defaultSide?: TradeSide;
  /** Called after a successful order. */
  onDone?: () => void;
  className?: string;
}

const MAX_QUANTITY = 1_000_000;

/**
 * Market order ticket: pick a side and a share count, see the estimated cost and the
 * resulting cash/shares before submitting. Prices are live, so the estimate moves.
 */
export const TradeTicket = ({ ticker, defaultSide = "buy", onDone, className }: TradeTicketProps) => {
  const trpc = useTRPC();
  const invalidateAccount = useInvalidateAccount();

  const [side, setSide] = useState<TradeSide>(defaultSide);
  const [quantityInput, setQuantityInput] = useState("1");

  const { quote, isLoading: isQuoteLoading, notFound } = useLiveQuote(ticker);
  const { data: budget } = useQuery(trpc.budgets.getOne.queryOptions());
  const { data: positions } = useQuery(trpc.portfolio.getPositions.queryOptions());

  const cash = budget?.amount ?? 0;
  const owned = positions?.find((position) => position.ticker === ticker)?.quantity ?? 0;
  const price = quote?.price;

  const quantity = Number(quantityInput);
  const isWholeNumber = /^\d+$/.test(quantityInput) && quantity >= 1 && quantity <= MAX_QUANTITY;
  const estimatedTotal = price !== undefined && isWholeNumber ? price * quantity : 0;
  const maxAffordable = price ? Math.floor(cash / price) : 0;

  const validationError = (() => {
    if (!quantityInput) return "Enter how many shares to trade.";
    if (!isWholeNumber) return "Use a whole number of shares (1 or more).";
    if (side === "buy" && price !== undefined && estimatedTotal > cash) {
      return maxAffordable > 0
        ? `Not enough cash. You can afford up to ${formatNumber(maxAffordable)} share${maxAffordable === 1 ? "" : "s"}.`
        : "Not enough cash to buy a single share.";
    }
    if (side === "sell" && owned === 0) return `You don't own any ${ticker}.`;
    if (side === "sell" && quantity > owned) {
      return `You only own ${formatNumber(owned)} share${owned === 1 ? "" : "s"}.`;
    }
    return null;
  })();

  const mutationCallbacks = {
    onSuccess: async ({ quantity: filled }: { quantity: number }) => {
      toast.success(`${side === "buy" ? "Bought" : "Sold"} ${formatNumber(filled)} ${ticker}`, {
        description: price ? `Filled near ${formatCurrency(price)} per share.` : undefined,
      });
      setQuantityInput("1");
      await invalidateAccount();
      onDone?.();
    },
    onError: (error: { message: string }) => {
      toast.error("Order rejected", { description: error.message });
    },
  };

  const buy = useMutation(trpc.transactions.buy.mutationOptions(mutationCallbacks));
  const sell = useMutation(trpc.transactions.sell.mutationOptions(mutationCallbacks));
  const isPending = buy.isPending || sell.isPending;

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (validationError || isPending || price === undefined) {
      return;
    }

    (side === "buy" ? buy : sell).mutate({ ticker, quantity });
  };

  const setQuantity = (value: number) => {
    setQuantityInput(String(Math.max(1, Math.min(MAX_QUANTITY, Math.floor(value)))));
  };

  if (notFound) {
    return (
      <div className={cn("rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground", className)}>
        No quote is available for <span className="font-semibold text-foreground">{ticker}</span>, so it
        can&apos;t be traded right now.
      </div>
    );
  }

  const presets =
    side === "buy"
      ? [
          { label: "1", value: 1 },
          { label: "5", value: 5 },
          { label: "10", value: 10 },
          { label: "Max", value: maxAffordable },
        ]
      : [
          { label: "25%", value: Math.floor(owned * 0.25) },
          { label: "50%", value: Math.floor(owned * 0.5) },
          { label: "All", value: owned },
        ];

  const cashAfter = side === "buy" ? cash - estimatedTotal : cash + estimatedTotal;
  const sharesAfter = isWholeNumber ? (side === "buy" ? owned + quantity : owned - quantity) : owned;

  return (
    <form onSubmit={handleSubmit} className={cn("flex flex-col gap-4", className)} noValidate>
      <div className="grid grid-cols-2 gap-1 rounded-lg bg-muted p-1" role="radiogroup" aria-label="Order side">
        {(["buy", "sell"] as const).map((option) => (
          <button
            key={option}
            type="button"
            role="radio"
            aria-checked={side === option}
            onClick={() => setSide(option)}
            className={cn(
              "h-9 rounded-md text-sm font-semibold capitalize transition-colors cursor-pointer",
              side === option
                ? option === "buy"
                  ? "bg-gain text-white shadow-sm"
                  : "bg-loss text-white shadow-sm"
                : "text-muted-foreground hover:text-foreground"
            )}
          >
            {option}
          </button>
        ))}
      </div>

      <div className="flex items-center justify-between text-sm">
        <span className="text-muted-foreground">Market price</span>
        <span className="font-semibold">
          {isQuoteLoading ? <LoaderIcon className="size-4 animate-spin" /> : <LivePrice quote={quote} />}
        </span>
      </div>

      <div className="flex flex-col gap-2">
        <Label htmlFor={`quantity-${ticker}`}>Shares</Label>
        <div className="flex items-center gap-2">
          <Button
            type="button"
            variant="outline"
            size="icon"
            className="size-10"
            onClick={() => setQuantity((isWholeNumber ? quantity : 1) - 1)}
            disabled={isPending || !isWholeNumber || quantity <= 1}
            aria-label="Decrease shares"
          >
            <MinusIcon />
          </Button>
          <Input
            id={`quantity-${ticker}`}
            inputMode="numeric"
            autoComplete="off"
            value={quantityInput}
            onChange={(event) => setQuantityInput(event.target.value.replace(/[^\d]/g, ""))}
            className="h-10 text-center text-lg font-semibold tabular-nums"
            disabled={isPending}
            aria-invalid={!!validationError}
            aria-describedby={`quantity-${ticker}-help`}
          />
          <Button
            type="button"
            variant="outline"
            size="icon"
            className="size-10"
            onClick={() => setQuantity((isWholeNumber ? quantity : 0) + 1)}
            disabled={isPending}
            aria-label="Increase shares"
          >
            <PlusIcon />
          </Button>
        </div>
        <div className="flex flex-wrap gap-1.5">
          {presets.map((preset) => (
            <Button
              key={preset.label}
              type="button"
              variant="outline"
              size="sm"
              className="h-7 px-2.5 text-xs"
              disabled={isPending || preset.value < 1}
              onClick={() => setQuantity(preset.value)}
            >
              {preset.label}
            </Button>
          ))}
        </div>
      </div>

      <dl className="flex flex-col gap-1.5 rounded-lg border bg-muted/40 p-3 text-sm">
        <div className="flex justify-between">
          <dt className="text-muted-foreground">{side === "buy" ? "Estimated cost" : "Estimated proceeds"}</dt>
          <dd className="font-semibold tabular-nums">{formatCurrency(estimatedTotal)}</dd>
        </div>
        <Separator className="my-1" />
        <div className="flex justify-between">
          <dt className="text-muted-foreground">Cash available</dt>
          <dd className="tabular-nums">{formatCurrency(cash)}</dd>
        </div>
        <div className="flex justify-between">
          <dt className="text-muted-foreground">Cash after order</dt>
          <dd className={cn("tabular-nums", cashAfter < 0 && "text-loss")}>{formatCurrency(cashAfter)}</dd>
        </div>
        <div className="flex justify-between">
          <dt className="text-muted-foreground">Shares owned</dt>
          <dd className="tabular-nums">
            {formatNumber(owned)}
            {isWholeNumber && sharesAfter !== owned && sharesAfter >= 0 && (
              <span className="text-muted-foreground"> → {formatNumber(sharesAfter)}</span>
            )}
          </dd>
        </div>
      </dl>

      <p
        id={`quantity-${ticker}-help`}
        className={cn("text-sm min-h-5", validationError ? "text-loss" : "text-muted-foreground")}
        role={validationError ? "alert" : undefined}
      >
        {validationError ?? (
          <span className="inline-flex items-start gap-1.5">
            <InfoIcon className="size-4 mt-0.5 shrink-0" aria-hidden="true" />
            Market order: fills at the live price when it reaches the exchange.
          </span>
        )}
      </p>

      <Button
        type="submit"
        size="lg"
        disabled={!!validationError || isPending || price === undefined}
        className={cn(
          "w-full border-transparent text-white font-semibold",
          side === "buy" ? "bg-gain hover:bg-gain/90" : "bg-loss hover:bg-loss/90"
        )}
      >
        {isPending && <LoaderIcon className="animate-spin" />}
        {side === "buy" ? "Buy" : "Sell"} {isWholeNumber ? formatNumber(quantity) : ""} {ticker}
      </Button>
    </form>
  );
};
