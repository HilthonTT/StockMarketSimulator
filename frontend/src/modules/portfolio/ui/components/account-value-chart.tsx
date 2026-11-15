"use client";

import { useState } from "react";

import { PriceChart } from "@/modules/market/ui/components/price-chart";
import { PriceChange } from "@/modules/market/ui/components/price-change";
import { HISTORY_RANGES, type HistoryRange } from "@/modules/stocks/types";

import { formatCurrency } from "@/lib/utils";
import { STARTING_CASH } from "@/constants";
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";

import type { Portfolio } from "../../hooks/use-portfolio";
import { usePortfolioHistory } from "../../hooks/use-portfolio-history";

/** Hero chart: what the account (cash + current holdings) was worth over the selected range. */
export const AccountValueChart = ({ portfolio, height = 280 }: { portfolio: Portfolio; height?: number }) => {
  const [range, setRange] = useState<HistoryRange>("1D");
  const points = usePortfolioHistory(portfolio, range);

  const start = points?.[0]?.price ?? portfolio.equity;
  const change = portfolio.equity - start;

  return (
    <Card className="gap-4">
      <CardHeader>
        <CardDescription>Account value</CardDescription>
        <CardTitle className="num text-3xl">{formatCurrency(portfolio.equity)}</CardTitle>
        <div className="flex flex-wrap items-center gap-x-3 gap-y-1 text-sm">
          <span className="inline-flex items-center gap-1">
            <PriceChange change={change} ratio={start ? change / start : 0} />
            <span className="text-muted-foreground">{range === "1D" ? "today" : `past ${range}`}</span>
          </span>
          <span className="inline-flex items-center gap-1">
            <PriceChange change={portfolio.allTimePnl} ratio={portfolio.allTimePnlRatio} showIcon={false} />
            <span className="text-muted-foreground">since the {formatCurrency(STARTING_CASH)} start</span>
          </span>
        </div>
        <CardAction>
          <ToggleGroup
            type="single"
            size="sm"
            variant="outline"
            value={range}
            onValueChange={(value) => value && setRange(value as HistoryRange)}
            aria-label="Account chart range"
          >
            {HISTORY_RANGES.map((option) => (
              <ToggleGroupItem key={option} value={option} className="num px-2.5 text-xs">
                {option}
              </ToggleGroupItem>
            ))}
          </ToggleGroup>
        </CardAction>
      </CardHeader>
      <CardContent>
        <PriceChart history={points} range={range} height={height} emptyLabel="Building your account history…" />
        <p className="mt-2 text-xs text-muted-foreground">Your current cash and holdings, valued at each point in time.</p>
      </CardContent>
    </Card>
  );
};
