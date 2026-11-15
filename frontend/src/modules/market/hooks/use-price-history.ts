"use client";

import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";

import type { HistoryRange } from "@/modules/stocks/types";

import { useTRPC } from "@/trpc/client";
import { parseUtc } from "@/lib/utils";

import { normalizeTicker, useFeedStore, type PricePoint } from "../store";

/**
 * Price series for a chart: stored bars from the API and, for the intraday range,
 * the live ticks received since, so the line keeps moving.
 */
export function usePriceHistory(
  rawTicker: string,
  range: HistoryRange,
  { live = range === "1D", enabled = true }: { live?: boolean; enabled?: boolean } = {}
) {
  const ticker = normalizeTicker(rawTicker);
  const trpc = useTRPC();

  const { data, isLoading, isError } = useQuery(
    trpc.stocks.getHistory.queryOptions(
      { ticker, range },
      { enabled: enabled && !!ticker, staleTime: range === "1D" ? 60_000 : 5 * 60_000 }
    )
  );

  const ticks = useFeedStore((state) => (live ? state.quotes[ticker]?.history : undefined));

  const points = useMemo<PricePoint[] | undefined>(() => {
    if (!data) {
      return undefined;
    }

    const bars = data.map((bar) => ({ time: parseUtc(bar.time).getTime(), price: bar.close }));
    const lastBarTime = bars.at(-1)?.time ?? 0;
    const newer = (ticks ?? []).filter((tick) => tick.time > lastBarTime);

    return [...bars, ...newer];
  }, [data, ticks]);

  return { points, isLoading, isError };
}

/**
 * Compact series for sparklines. Built-in listings come from the market overview (one request for
 * every row); other symbols fall back to their own 1D history. The live price is appended.
 */
export function useSparkline(rawTicker: string, maxPoints = 48) {
  const ticker = normalizeTicker(rawTicker);
  const trpc = useTRPC();

  const { data: market } = useQuery(trpc.stocks.getMarket.queryOptions(undefined, { staleTime: 60_000 }));
  const fromMarket = market?.sparklines?.[ticker];

  const { points } = usePriceHistory(ticker, "1D", { live: false, enabled: !!market && !fromMarket });
  const livePrice = useFeedStore((state) => state.quotes[ticker]?.price);

  return useMemo(() => {
    const series = fromMarket ?? points?.map((point) => point.price) ?? [];
    if (series.length === 0) {
      return [];
    }

    const stride = Math.max(1, Math.ceil(series.length / maxPoints));
    const sampled = series.filter((_, index) => index % stride === 0);
    const last = livePrice ?? series[series.length - 1];

    if (sampled[sampled.length - 1] !== last) {
      sampled.push(last);
    }

    return sampled;
  }, [fromMarket, points, livePrice, maxPoints]);
}
