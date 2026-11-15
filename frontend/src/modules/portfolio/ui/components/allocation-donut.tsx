"use client";

import { Cell, Pie, PieChart, ResponsiveContainer, Tooltip } from "recharts";

import { formatCurrency, formatPercent } from "@/lib/utils";
import { displaySymbol, isCryptoTicker } from "@/lib/assets";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

import type { Portfolio } from "../../hooks/use-portfolio";

// Distinct, theme-aware hues; cash is always neutral.
const SEGMENT_COLORS = [
  "var(--chart-1)",
  "var(--chart-2)",
  "var(--chart-3)",
  "var(--chart-4)",
  "var(--chart-5)",
  "var(--chart-6)",
];
const MAX_SEGMENTS = 6;

type Segment = { label: string; value: number; color: string };

const buildSegments = (portfolio: Portfolio): Segment[] => {
  const top = portfolio.holdings.slice(0, MAX_SEGMENTS - 1);
  const rest = portfolio.holdings.slice(MAX_SEGMENTS - 1);

  const segments: Segment[] = top.map((holding, index) => ({
    label: displaySymbol(holding.ticker),
    value: holding.marketValue,
    color: SEGMENT_COLORS[index % SEGMENT_COLORS.length],
  }));

  if (rest.length > 0) {
    segments.push({
      label: `${rest.length} other${rest.length === 1 ? "" : "s"}`,
      value: rest.reduce((sum, holding) => sum + holding.marketValue, 0),
      color: SEGMENT_COLORS[MAX_SEGMENTS - 1],
    });
  }

  segments.push({ label: "Cash", value: portfolio.cash, color: "var(--muted-foreground)" });

  return segments.filter((segment) => segment.value > 0);
};

/** Donut of holdings + cash, with stock/crypto/cash totals underneath. */
export const AllocationDonut = ({ portfolio }: { portfolio: Portfolio }) => {
  const segments = buildSegments(portfolio);
  const equity = portfolio.equity || 1;

  const crypto = portfolio.holdings
    .filter((holding) => holding.quote?.assetClass === "crypto" || isCryptoTicker(holding.ticker))
    .reduce((sum, holding) => sum + holding.marketValue, 0);

  const classes = [
    { label: "Stocks", value: portfolio.investedValue - crypto },
    { label: "Crypto", value: crypto },
    { label: "Cash", value: portfolio.cash },
  ];

  return (
    <Card className="gap-4">
      <CardHeader>
        <CardTitle>Allocation</CardTitle>
        <CardDescription>How your {formatCurrency(portfolio.equity)} is split.</CardDescription>
      </CardHeader>
      <CardContent className="flex flex-col gap-5">
        <div
          className="relative mx-auto aspect-square w-full max-w-[220px]"
          role="img"
          aria-label={segments
            .map((segment) => `${segment.label} ${formatPercent(segment.value / equity, { signed: false })}`)
            .join(", ")}
        >
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie
                data={segments}
                dataKey="value"
                nameKey="label"
                innerRadius="68%"
                outerRadius="100%"
                paddingAngle={segments.length > 1 ? 2 : 0}
                stroke="none"
                isAnimationActive={false}
              >
                {segments.map((segment) => (
                  <Cell key={segment.label} fill={segment.color} />
                ))}
              </Pie>
              <Tooltip
                formatter={(value) => formatCurrency(Number(value))}
                contentStyle={{
                  background: "var(--popover)",
                  border: "1px solid var(--border)",
                  borderRadius: 8,
                  color: "var(--popover-foreground)",
                }}
              />
            </PieChart>
          </ResponsiveContainer>
          <div className="pointer-events-none absolute inset-0 flex flex-col items-center justify-center">
            <span className="text-xs text-muted-foreground">Invested</span>
            <span className="num text-lg font-bold">
              {formatPercent(portfolio.investedValue / equity, { signed: false })}
            </span>
          </div>
        </div>

        <ul className="grid gap-2 text-sm">
          {segments.map((segment) => (
            <li key={segment.label} className="flex items-center gap-2">
              <span className="size-2.5 rounded-full" style={{ backgroundColor: segment.color }} aria-hidden="true" />
              <span className="flex-1 font-medium">{segment.label}</span>
              <span className="num text-muted-foreground">{formatCurrency(segment.value)}</span>
              <span className="num w-16 text-right">{formatPercent(segment.value / equity, { signed: false })}</span>
            </li>
          ))}
        </ul>

        <div className="grid grid-cols-3 gap-2 border-t pt-4 text-center">
          {classes.map((assetClass) => (
            <div key={assetClass.label}>
              <p className="text-xs text-muted-foreground">{assetClass.label}</p>
              <p className="num text-sm font-semibold">{formatPercent(assetClass.value / equity, { signed: false })}</p>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
};
