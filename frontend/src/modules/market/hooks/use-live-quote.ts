"use client";

import { useContext, useEffect, useMemo } from "react";
import { useQueries, useQuery } from "@tanstack/react-query";
import { useShallow } from "zustand/react/shallow";

import { useTRPC } from "@/trpc/client";

import { StockFeedContext } from "../providers/stock-feed-provider";
import { normalizeTicker, useFeedStore, type Quote } from "../store";

/** Keeps live ticks flowing for the given tickers while the caller is mounted. */
function useFeedSubscriptions(tickers: string[]) {
  const feed = useContext(StockFeedContext);
  const key = tickers.join(",");

  useEffect(() => {
    if (!feed || !key) {
      return;
    }

    const unsubscribers = key.split(",").map((ticker) => feed.subscribe(ticker));

    return () => unsubscribers.forEach((unsubscribe) => unsubscribe());
  }, [feed, key]);
}

/**
 * Live quote for one ticker: seeded from the REST quote, then updated by the feed.
 * `quote` is undefined until the first price is known; `notFound` is true when the
 * API doesn't know the ticker.
 */
export function useLiveQuote(rawTicker: string | undefined) {
  const ticker = rawTicker ? normalizeTicker(rawTicker) : "";
  const trpc = useTRPC();
  const seed = useFeedStore((state) => state.seed);

  const { data, isLoading, isError } = useQuery(
    trpc.stocks.getOne.queryOptions({ ticker }, { enabled: !!ticker })
  );

  useEffect(() => {
    if (data) {
      seed(data.ticker, data.price);
    }
  }, [data, seed]);

  const tickers = useMemo(() => (ticker ? [ticker] : []), [ticker]);
  useFeedSubscriptions(tickers);

  const quote = useFeedStore((state) => (ticker ? state.quotes[ticker] : undefined));

  return {
    quote,
    isLoading: isLoading && !quote,
    isError: isError && !quote,
    notFound: !!ticker && !isLoading && !isError && data === null && !quote,
  };
}

/** Live quotes for several tickers at once, keyed by upper-case ticker. */
export function useLiveQuotes(rawTickers: string[]) {
  const key = [...new Set(rawTickers.map(normalizeTicker))].sort().join(",");
  const tickers = useMemo(() => (key ? key.split(",") : []), [key]);

  const trpc = useTRPC();
  const seed = useFeedStore((state) => state.seed);

  const results = useQueries({
    queries: tickers.map((ticker) => trpc.stocks.getOne.queryOptions({ ticker })),
  });

  const seedKey = results
    .map((result) => (result.data ? `${result.data.ticker}:${result.data.price}` : ""))
    .join("|");

  useEffect(() => {
    if (!seedKey) {
      return;
    }

    for (const entry of seedKey.split("|")) {
      const [ticker, price] = entry.split(":");
      if (ticker && price) {
        seed(ticker, Number(price));
      }
    }
  }, [seedKey, seed]);

  useFeedSubscriptions(tickers);

  return useFeedStore(
    useShallow((state) => {
      const quotes: Record<string, Quote | undefined> = {};
      for (const ticker of tickers) {
        quotes[ticker] = state.quotes[ticker];
      }
      return quotes;
    })
  );
}
