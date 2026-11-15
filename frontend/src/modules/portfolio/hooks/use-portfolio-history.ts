"use client";

import { useMemo } from "react";
import { useQueries } from "@tanstack/react-query";

import type { HistoryRange, PriceBar } from "@/modules/stocks/types";
import type { PricePoint } from "@/modules/market/store";

import { useTRPC } from "@/trpc/client";
import { parseUtc } from "@/lib/utils";

import type { Portfolio } from "./use-portfolio";

const combineHistories = (results: { data?: PriceBar[] }[]) => results.map((result) => result.data);

/**
 * Account value over a range: today's cash plus today's holdings valued at each historical
 * price. It shows how the current portfolio moved, not a replay of past trades (the API
 * keeps no account snapshots). The last point is the live account value.
 */
export function usePortfolioHistory(portfolio: Portfolio, range: HistoryRange) {
  const trpc = useTRPC();
  const { holdings, cash, equity } = portfolio;

  const histories = useQueries({
    queries: holdings.map((holding) =>
      trpc.stocks.getHistory.queryOptions({ ticker: holding.ticker, range }, { staleTime: 60_000 })
    ),
    combine: combineHistories,
  });

  return useMemo<PricePoint[] | undefined>(() => {
    if (holdings.length === 0 || histories.some((history) => history === undefined)) {
      return undefined;
    }

    const series = histories.map((history) =>
      (history ?? []).map((bar) => ({ time: parseUtc(bar.time).getTime(), price: bar.close }))
    );

    // The densest series is the timeline; every holding's price is forward-filled onto it.
    const timeline = series.reduce((longest, current) => (current.length > longest.length ? current : longest), []);
    if (timeline.length === 0) {
      return [];
    }

    const cursors = series.map(() => 0);

    const values = timeline.map(({ time }) => {
      let value = cash;

      series.forEach((points, index) => {
        while (cursors[index] + 1 < points.length && points[cursors[index] + 1].time <= time) {
          cursors[index] += 1;
        }

        const price = points[cursors[index]]?.price ?? holdings[index].price;
        value += price * holdings[index].quantity;
      });

      return { time, price: value };
    });

    // The live point is stamped with the latest tick so the series stays pure.
    const latestTick = Math.max(...holdings.map((holding) => holding.quote?.updatedAt ?? 0));
    const lastTime = values[values.length - 1].time;

    return latestTick > lastTime ? [...values, { time: latestTick, price: equity }] : values;
  }, [holdings, histories, cash, equity]);
}
