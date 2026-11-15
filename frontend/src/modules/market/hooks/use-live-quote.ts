"use client";

import { useContext, useEffect, useMemo } from "react";
import { useQueries, useQuery } from "@tanstack/react-query";
import { useShallow } from "zustand/react/shallow";

import type { StockQuoteResponse } from "@/modules/stocks/types";

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
      seed(data);
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

// Module-level so useQueries can keep the combined array stable between renders.
const combineQuotes = (results: { data?: StockQuoteResponse | null }[]) =>
  results.flatMap((result) => (result.data ? [result.data] : []));

/** Seeds the feed store from an already-loaded list of quotes (e.g. the market overview). */
export function useSeedQuotes(quotes: StockQuoteResponse[] | undefined) {
  const seed = useFeedStore((state) => state.seed);

  useEffect(() => {
    for (const quote of quotes ?? []) {
      seed(quote);
    }
  }, [quotes, seed]);
}

/** Keeps live ticks flowing for tickers whose quotes were seeded elsewhere. */
export const useLiveSubscriptions = (rawTickers: string[]) => {
  const key = [...new Set(rawTickers.map(normalizeTicker))].sort().join(",");
  const tickers = useMemo(() => (key ? key.split(",") : []), [key]);
  useFeedSubscriptions(tickers);
};

/** Live quotes for several tickers at once, keyed by upper-case ticker. */
export function useLiveQuotes(rawTickers: string[]) {
  const key = [...new Set(rawTickers.map(normalizeTicker))].sort().join(",");
  const tickers = useMemo(() => (key ? key.split(",") : []), [key]);

  const trpc = useTRPC();
  const seed = useFeedStore((state) => state.seed);

  const seeds = useQueries({
    queries: tickers.map((ticker) => trpc.stocks.getOne.queryOptions({ ticker })),
    combine: combineQuotes,
  });

  useEffect(() => {
    for (const quote of seeds) {
      seed(quote);
    }
  }, [seeds, seed]);

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
