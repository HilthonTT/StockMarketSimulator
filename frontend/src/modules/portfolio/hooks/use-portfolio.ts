"use client";

import { useMemo } from "react";
import { useSuspenseQuery } from "@tanstack/react-query";

import { useLiveQuotes } from "@/modules/market/hooks/use-live-quote";
import type { Quote } from "@/modules/market/store";

import { useTRPC } from "@/trpc/client";
import { ratio } from "@/lib/utils";
import { STARTING_CASH } from "@/constants";

export type Holding = {
  ticker: string;
  quantity: number;
  averageCost: number;
  totalCost: number;
  quote: Quote | undefined;
  /** Live price, or the average cost until the first quote arrives. */
  price: number;
  marketValue: number;
  unrealizedPnl: number;
  unrealizedPnlRatio: number;
  /** Change in value since the session's first price for this ticker. */
  sessionPnl: number;
  /** Share of the invested (non-cash) value. */
  allocation: number;
};

/**
 * The user's account valued at live prices: cash, positions and the derived totals.
 * Suspends until positions and cash are loaded.
 */
export function usePortfolio() {
  const trpc = useTRPC();

  const { data: positions } = useSuspenseQuery(trpc.portfolio.getPositions.queryOptions());
  const { data: budget } = useSuspenseQuery(trpc.budgets.getOne.queryOptions());

  const tickers = useMemo(() => positions.map((position) => position.ticker), [positions]);
  const quotes = useLiveQuotes(tickers);

  return useMemo(() => {
    const valued = positions.map((position) => {
      const quote = quotes[position.ticker];
      const price = quote?.price ?? position.averageCost;
      const marketValue = price * position.quantity;
      const unrealizedPnl = marketValue - position.totalCost;

      return {
        ...position,
        quote,
        price,
        marketValue,
        unrealizedPnl,
        unrealizedPnlRatio: ratio(unrealizedPnl, position.totalCost),
        sessionPnl: quote ? (quote.price - quote.sessionOpen) * position.quantity : 0,
      };
    });

    const investedValue = valued.reduce((sum, holding) => sum + holding.marketValue, 0);
    const costBasis = valued.reduce((sum, holding) => sum + holding.totalCost, 0);
    const sessionPnl = valued.reduce((sum, holding) => sum + holding.sessionPnl, 0);
    const cash = budget.amount;
    const equity = cash + investedValue;
    const unrealizedPnl = investedValue - costBasis;
    const allTimePnl = equity - STARTING_CASH;

    const holdings: Holding[] = valued
      .map((holding) => ({ ...holding, allocation: ratio(holding.marketValue, investedValue) }))
      .sort((a, b) => b.marketValue - a.marketValue);

    return {
      holdings,
      cash,
      currencyCode: budget.currencyCode,
      investedValue,
      costBasis,
      equity,
      unrealizedPnl,
      unrealizedPnlRatio: ratio(unrealizedPnl, costBasis),
      sessionPnl,
      allTimePnl,
      allTimePnlRatio: ratio(allTimePnl, STARTING_CASH),
      cashRatio: ratio(cash, equity),
      pricesReady: valued.every((holding) => holding.quote !== undefined),
    };
  }, [positions, budget, quotes]);
}

export type Portfolio = ReturnType<typeof usePortfolio>;
