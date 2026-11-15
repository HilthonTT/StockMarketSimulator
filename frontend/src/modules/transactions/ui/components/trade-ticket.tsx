"use client";

import { useState } from "react";
import { InfoIcon, LoaderIcon, MinusIcon, PlusIcon } from "lucide-react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

import { useLiveQuote } from "@/modules/market/hooks/use-live-quote";
import { LivePrice } from "@/modules/market/ui/components/live-price";
import { describeOrder, fillCondition } from "@/modules/orders/hooks/use-orders";

import { useTRPC } from "@/trpc/client";
import { cn, formatCurrency, formatNumber } from "@/lib/utils";
import { displaySymbol, isCryptoTicker } from "@/lib/assets";
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

type OrderKind = "market" | "limit" | "stop";

const MAX_QUANTITY = 1_000_000;

const ORDER_KINDS: { value: OrderKind; label: string }[] = [
  { value: "market", label: "Market" },
  { value: "limit", label: "Limit" },
  { value: "stop", label: "Stop" },
];

/** A sensible starting trigger: a limit waits for a better price, a stop guards against a move. */
const defaultTrigger = (kind: OrderKind, side: TradeSide, price: number) => {
  const factor =
    kind === "limit" ? (side === "buy" ? 0.99 : 1.01) : side === "buy" ? 1.03 : 0.95;

  return (Math.round(price * factor * 100) / 100).toFixed(2);
};

const explain = (kind: OrderKind, side: TradeSide, unit: string) => {
  switch (kind) {
    case "market":
      return "Market order: fills right away at the live price.";
    case "limit":
      return side === "buy"
        ? `Limit buy: waits until the price drops to your limit or lower, then buys at the market price.`
        : `Limit sell: waits until the price rises to your limit or higher, then sells at the market price.`;
    case "stop":
      return side === "buy"
        ? `Stop buy: buys once the price climbs to your stop, e.g. to catch a breakout.`
        : `Stop-loss: sells your ${unit} once the price falls to your stop, limiting the loss.`;
  }
};

/**
 * Order ticket: market orders fill immediately; limit and stop orders rest until the live
 * price crosses the trigger. Shows the estimated cost and resulting cash/holdings before submitting.
 */
export const TradeTicket = ({ ticker, defaultSide = "buy", onDone, className }: TradeTicketProps) => {
  const trpc = useTRPC();
  const queryClient = useQueryClient();
  const invalidateAccount = useInvalidateAccount();

  const [side, setSide] = useState<TradeSide>(defaultSide);
  const [kind, setKind] = useState<OrderKind>("market");
  const [quantityInput, setQuantityInput] = useState("1");
  const [triggerInput, setTriggerInput] = useState("");

  const { quote, isLoading: isQuoteLoading, notFound } = useLiveQuote(ticker);
  const { data: budget } = useQuery(trpc.budgets.getOne.queryOptions());
  const { data: positions } = useQuery(trpc.portfolio.getPositions.queryOptions());

  const isCrypto = quote?.assetClass === "crypto" || isCryptoTicker(ticker);
  const unit = isCrypto ? "coins" : "shares";
  const symbol = displaySymbol(ticker);

  const cash = budget?.amount ?? 0;
  const owned = positions?.find((position) => position.ticker === ticker)?.quantity ?? 0;
  const price = quote?.price;

  const quantity = Number(quantityInput);
  const isWholeNumber = /^\d+$/.test(quantityInput) && quantity >= 1 && quantity <= MAX_QUANTITY;

  const trigger = Number(triggerInput);
  const isTriggerValid = kind === "market" || (/^\d+(\.\d{1,2})?$/.test(triggerInput) && trigger > 0);

  // Market orders estimate at the live price; resting orders at their trigger.
  const estimatePrice = kind === "market" ? price : isTriggerValid ? trigger : undefined;
  const estimatedTotal = estimatePrice !== undefined && isWholeNumber ? estimatePrice * quantity : 0;
  const maxAffordable = estimatePrice ? Math.floor(cash / estimatePrice) : 0;

  const validationError = (() => {
    if (!quantityInput) return `Enter how many ${unit} to trade.`;
    if (!isWholeNumber) return `Use a whole number of ${unit} (1 or more).`;
    if (kind !== "market" && !isTriggerValid) return "Enter a trigger price above $0 (up to 2 decimals).";
    if (side === "buy" && estimatePrice !== undefined && estimatedTotal > cash) {
      return maxAffordable > 0
        ? `Not enough cash. You can afford up to ${formatNumber(maxAffordable)} ${unit}.`
        : `Not enough cash for a single unit at ${formatCurrency(estimatePrice)}.`;
    }
    if (side === "sell" && owned === 0) return `You don't own any ${symbol}.`;
    if (side === "sell" && quantity > owned) return `You only own ${formatNumber(owned)} ${unit}.`;
    return null;
  })();

  const onError = (error: { message: string }) => {
    toast.error("Order rejected", { description: error.message });
  };

  const tradeCallbacks = {
    onSuccess: async ({ quantity: filled }: { quantity: number }) => {
      toast.success(`${side === "buy" ? "Bought" : "Sold"} ${formatNumber(filled)} ${symbol}`, {
        description: price ? `Filled near ${formatCurrency(price)} each.` : undefined,
      });
      setQuantityInput("1");
      await invalidateAccount();
      onDone?.();
    },
    onError,
  };

  const buy = useMutation(trpc.transactions.buy.mutationOptions(tradeCallbacks));
  const sell = useMutation(trpc.transactions.sell.mutationOptions(tradeCallbacks));
  const place = useMutation(
    trpc.orders.place.mutationOptions({
      onSuccess: async (order) => {
        toast.success("Order placed", {
          description: `${describeOrder(order)}. It fills when the price is ${fillCondition(order.side, order.type)} your trigger.`,
        });
        setQuantityInput("1");
        await queryClient.invalidateQueries(trpc.orders.getMany.pathFilter());
        onDone?.();
      },
      onError,
    })
  );
  const isPending = buy.isPending || sell.isPending || place.isPending;

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (validationError || isPending || price === undefined) {
      return;
    }

    if (kind === "market") {
      (side === "buy" ? buy : sell).mutate({ ticker, quantity });
    } else {
      place.mutate({ ticker, side, type: kind, quantity, triggerPrice: trigger });
    }
  };

  const setQuantity = (value: number) => {
    setQuantityInput(String(Math.max(1, Math.min(MAX_QUANTITY, Math.floor(value)))));
  };

  const chooseKind = (next: OrderKind) => {
    setKind(next);
    if (next !== "market" && price !== undefined) {
      setTriggerInput(defaultTrigger(next, side, price));
    }
  };

  const chooseSide = (next: TradeSide) => {
    setSide(next);
    if (kind !== "market" && price !== undefined) {
      setTriggerInput(defaultTrigger(kind, next, price));
    }
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
  const holdingsAfter = isWholeNumber ? (side === "buy" ? owned + quantity : owned - quantity) : owned;
  const submitLabel =
    kind === "market"
      ? `${side === "buy" ? "Buy" : "Sell"} ${isWholeNumber ? formatNumber(quantity) : ""} ${symbol}`
      : `Place ${kind} ${side}`;

  return (
    <form onSubmit={handleSubmit} className={cn("flex flex-col gap-4", className)} noValidate>
      <div className="grid grid-cols-2 gap-1 rounded-lg bg-muted p-1" role="radiogroup" aria-label="Order side">
        {(["buy", "sell"] as const).map((option) => (
          <button
            key={option}
            type="button"
            role="radio"
            aria-checked={side === option}
            onClick={() => chooseSide(option)}
            className={cn(
              "h-9 cursor-pointer rounded-md text-sm font-semibold capitalize transition-colors",
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

      <div className="flex gap-1 border-b" role="radiogroup" aria-label="Order type">
        {ORDER_KINDS.map((option) => (
          <button
            key={option.value}
            type="button"
            role="radio"
            aria-checked={kind === option.value}
            onClick={() => chooseKind(option.value)}
            className={cn(
              "-mb-px cursor-pointer border-b-2 px-3 pb-2 text-sm font-medium transition-colors",
              kind === option.value
                ? "border-primary text-foreground"
                : "border-transparent text-muted-foreground hover:text-foreground"
            )}
          >
            {option.label}
          </button>
        ))}
      </div>

      <div className="flex items-center justify-between text-sm">
        <span className="text-muted-foreground">Market price</span>
        <span className="num font-semibold">
          {isQuoteLoading ? <LoaderIcon className="size-4 animate-spin" /> : <LivePrice quote={quote} />}
        </span>
      </div>

      {kind !== "market" && (
        <div className="flex flex-col gap-2">
          <Label htmlFor={`trigger-${ticker}`}>{kind === "limit" ? "Limit price" : "Stop price"}</Label>
          <div className="relative">
            <span className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground">$</span>
            <Input
              id={`trigger-${ticker}`}
              inputMode="decimal"
              autoComplete="off"
              value={triggerInput}
              onChange={(event) => setTriggerInput(event.target.value.replace(/[^\d.]/g, ""))}
              className="num h-10 pl-7 text-lg font-semibold"
              disabled={isPending}
              aria-invalid={!isTriggerValid}
            />
          </div>
          {price !== undefined && isTriggerValid && (
            <p className="text-xs text-muted-foreground">
              Fills when the price is {fillCondition(side, kind)} {formatCurrency(trigger)} (
              <span className={cn("num", trigger >= price ? "text-gain" : "text-loss")}>
                {trigger >= price ? "+" : ""}
                {(((trigger - price) / price) * 100).toFixed(2)}%
              </span>{" "}
              from now).
            </p>
          )}
        </div>
      )}

      <div className="flex flex-col gap-2">
        <Label htmlFor={`quantity-${ticker}`}>{isCrypto ? "Coins" : "Shares"}</Label>
        <div className="flex items-center gap-2">
          <Button
            type="button"
            variant="outline"
            size="icon"
            className="size-10"
            onClick={() => setQuantity((isWholeNumber ? quantity : 1) - 1)}
            disabled={isPending || !isWholeNumber || quantity <= 1}
            aria-label={`Decrease ${unit}`}
          >
            <MinusIcon />
          </Button>
          <Input
            id={`quantity-${ticker}`}
            inputMode="numeric"
            autoComplete="off"
            value={quantityInput}
            onChange={(event) => setQuantityInput(event.target.value.replace(/[^\d]/g, ""))}
            className="num h-10 text-center text-lg font-semibold"
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
            aria-label={`Increase ${unit}`}
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
          <dd className="num font-semibold">{formatCurrency(estimatedTotal)}</dd>
        </div>
        <Separator className="my-1" />
        <div className="flex justify-between">
          <dt className="text-muted-foreground">Cash available</dt>
          <dd className="num">{formatCurrency(cash)}</dd>
        </div>
        <div className="flex justify-between">
          <dt className="text-muted-foreground">{kind === "market" ? "Cash after order" : "Cash after fill"}</dt>
          <dd className={cn("num", cashAfter < 0 && "text-loss")}>{formatCurrency(cashAfter)}</dd>
        </div>
        <div className="flex justify-between">
          <dt className="text-muted-foreground">{isCrypto ? "Coins owned" : "Shares owned"}</dt>
          <dd className="num">
            {formatNumber(owned)}
            {isWholeNumber && holdingsAfter !== owned && holdingsAfter >= 0 && (
              <span className="text-muted-foreground"> → {formatNumber(holdingsAfter)}</span>
            )}
          </dd>
        </div>
      </dl>

      <p
        id={`quantity-${ticker}-help`}
        className={cn("min-h-5 text-sm", validationError ? "text-loss" : "text-muted-foreground")}
        role={validationError ? "alert" : undefined}
      >
        {validationError ?? (
          <span className="inline-flex items-start gap-1.5">
            <InfoIcon className="mt-0.5 size-4 shrink-0" aria-hidden="true" />
            {explain(kind, side, unit)}
          </span>
        )}
      </p>

      <Button
        type="submit"
        size="lg"
        disabled={!!validationError || isPending || price === undefined}
        className={cn(
          "w-full border-transparent font-semibold text-white",
          side === "buy" ? "bg-gain hover:bg-gain/90" : "bg-loss hover:bg-loss/90"
        )}
      >
        {isPending && <LoaderIcon className="animate-spin" />}
        {submitLabel}
      </Button>
    </form>
  );
};
