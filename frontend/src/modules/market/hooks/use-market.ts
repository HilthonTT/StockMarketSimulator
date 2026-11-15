"use client";

import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { useShallow } from "zustand/react/shallow";

import type { MarketIndex } from "@/modules/stocks/types";

import { useTRPC } from "@/trpc/client";

import { useFeedStore, type Quote } from "../store";
import { useLiveSubscriptions, useSeedQuotes } from "./use-live-quote";

export type MarketOverview = {
  index: MarketIndex;
  quotes: Quote[];
  stocks: Quote[];
  crypto: Quote[];
  gainers: Quote[];
  losers: Quote[];
};

/**
 * The simulated universe at live prices. The index is price-weighted (like the API's SIM 30),
 * so it's recomputed from the live constituent prices.
 */
export function useMarket() {
  const trpc = useTRPC();
  const { data, isLoading, isError } = useQuery(
    trpc.stocks.getMarket.queryOptions(undefined, { staleTime: 60_000 })
  );

  useSeedQuotes(data?.quotes);

  const tickers = useMemo(() => data?.quotes.map((quote) => quote.ticker) ?? [], [data]);
  useLiveSubscriptions(tickers);

  const live = useFeedStore(
    useShallow((state) => tickers.map((ticker) => state.quotes[ticker]).filter((quote) => quote !== undefined))
  );

  const market = useMemo<MarketOverview | null>(() => {
    if (!data) {
      return null;
    }

    const restIndex = data.index;
    const constituents = live.filter((quote) => quote.assetClass === "stock");
    let index = restIndex;

    // Price-weighted like the API: sum of stock prices over a fixed divisor, recovered from the snapshot.
    const restSum = data.quotes
      .filter((quote) => quote.assetClass === "stock")
      .reduce((sum, quote) => sum + quote.price, 0);
    const divisor = restIndex.value > 0 ? restSum / restIndex.value : 0;

    if (constituents.length > 0 && divisor > 0) {
      const value = constituents.reduce((sum, quote) => sum + quote.price, 0) / divisor;
      const previousClose = constituents.reduce((sum, quote) => sum + quote.previousClose, 0) / divisor;
      const change = value - previousClose;

      index = { ...restIndex, value, change, changePercent: previousClose > 0 ? change / previousClose : 0 };
    }

    const byChange = [...live].sort((a, b) => b.changePercent - a.changePercent);

    return {
      index,
      quotes: live,
      stocks: live.filter((quote) => quote.assetClass === "stock"),
      crypto: live.filter((quote) => quote.assetClass === "crypto"),
      gainers: byChange.filter((quote) => quote.changePercent > 0).slice(0, 5),
      losers: byChange.filter((quote) => quote.changePercent < 0).reverse().slice(0, 5),
    };
  }, [data, live]);

  return { market, isLoading, isError };
}
