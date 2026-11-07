"use client";

import Link from "next/link";
import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";

import { useLiveQuotes } from "@/modules/market/hooks/use-live-quote";
import { useWatchlist } from "@/modules/market/hooks/use-watchlist";
import { PercentChange } from "@/modules/market/ui/components/price-change";

import { useTRPC } from "@/trpc/client";
import { formatCurrency, ratio } from "@/lib/utils";

/** Scrolling strip of live prices for the user's holdings and watchlist. */
export const TickerTape = () => {
  const trpc = useTRPC();
  const { data: positions } = useQuery(trpc.portfolio.getPositions.queryOptions());
  const watchlist = useWatchlist((state) => state.tickers);

  const tickers = useMemo(
    () => [...new Set([...(positions ?? []).map((position) => position.ticker), ...watchlist])],
    [positions, watchlist]
  );

  const quotes = useLiveQuotes(tickers);
  const items = tickers.map((ticker) => quotes[ticker]).filter((quote) => quote !== undefined);

  if (items.length === 0) {
    return null;
  }

  // Short lists don't need to scroll; long ones loop seamlessly (content rendered twice).
  const shouldScroll = items.length > 4;
  const loop = shouldScroll ? [...items, ...items] : items;

  return (
    <div className="relative overflow-hidden border-b bg-muted/40" aria-label="Live prices">
      <div
        className={
          shouldScroll
            ? "flex w-max animate-marquee hover:[animation-play-state:paused]"
            : "flex flex-wrap"
        }
      >
        {loop.map((quote, index) => (
          <Link
            key={`${quote.ticker}-${index}`}
            href={`/stocks/${quote.ticker}`}
            className="flex items-center gap-2 px-4 py-1.5 text-xs hover:bg-muted"
            aria-hidden={index >= items.length ? true : undefined}
            tabIndex={index >= items.length ? -1 : undefined}
          >
            <span className="font-semibold">{quote.ticker}</span>
            <span className="tabular-nums">{formatCurrency(quote.price)}</span>
            <PercentChange ratio={ratio(quote.price - quote.sessionOpen, quote.sessionOpen)} />
          </Link>
        ))}
      </div>
    </div>
  );
};
