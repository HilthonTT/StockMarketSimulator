"use client";

import Link from "next/link";
import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";

import { useLiveQuotes } from "@/modules/market/hooks/use-live-quote";
import { useMarket } from "@/modules/market/hooks/use-market";
import { useWatchlist } from "@/modules/market/hooks/use-watchlist";
import { PercentChange } from "@/modules/market/ui/components/price-change";

import { useTRPC } from "@/trpc/client";
import { formatCurrency } from "@/lib/utils";
import { assetHref, displaySymbol } from "@/lib/assets";
import { useHydrated } from "@/hooks/use-hydrated";

/** Scrolling strip of live prices: your holdings and watchlist first, then the whole market. */
export const TickerTape = () => {
  const trpc = useTRPC();
  const { data: positions } = useQuery(trpc.portfolio.getPositions.queryOptions());
  const watchlist = useWatchlist((state) => state.tickers);
  const { market } = useMarket();

  const personal = useMemo(
    () => [...new Set([...(positions ?? []).map((position) => position.ticker), ...watchlist])],
    [positions, watchlist]
  );

  const personalQuotes = useLiveQuotes(personal);

  const items = useMemo(() => {
    const first = personal.map((ticker) => personalQuotes[ticker]).filter((quote) => quote !== undefined);
    const seen = new Set(first.map((quote) => quote.ticker));

    return [...first, ...(market?.quotes ?? []).filter((quote) => !seen.has(quote.ticker))];
  }, [personal, personalQuotes, market]);

  // Positions and the (localStorage) watchlist differ between server and client: render after hydration.
  const hydrated = useHydrated();

  if (!hydrated || items.length === 0) {
    return null;
  }

  // Short lists don't need to scroll; long ones loop seamlessly (content rendered twice).
  const shouldScroll = items.length > 4;
  const loop = shouldScroll ? [...items, ...items] : items;

  return (
    <div className="relative overflow-hidden border-b bg-card/60" aria-label="Live prices">
      <div
        className={
          shouldScroll
            ? "flex w-max animate-marquee hover:[animation-play-state:paused] focus-within:[animation-play-state:paused]"
            : "flex flex-wrap"
        }
      >
        {loop.map((quote, index) => (
          <Link
            key={`${quote.ticker}-${index}`}
            href={assetHref(quote.ticker)}
            className="flex items-center gap-2 border-r px-4 py-1.5 text-xs hover:bg-muted"
            aria-hidden={index >= items.length ? true : undefined}
            tabIndex={index >= items.length ? -1 : undefined}
          >
            <span className="font-semibold">{displaySymbol(quote.ticker)}</span>
            <span className="num">{formatCurrency(quote.price)}</span>
            <PercentChange ratio={quote.changePercent} className="num" />
          </Link>
        ))}
      </div>
    </div>
  );
};
