"use client";

import Link from "next/link";
import { BriefcaseBusinessIcon, GlobeIcon } from "lucide-react";

import { AccountSummarySection } from "@/modules/dashboard/ui/sections/account-summary-section";
import { LivePrice } from "@/modules/market/ui/components/live-price";
import { PercentChange, PriceChange } from "@/modules/market/ui/components/price-change";
import { useTradeDialog } from "@/modules/transactions/hooks/use-trade-dialog";

import { formatCurrency, formatNumber, formatPercent } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableFooter, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { PageHeader } from "@/components/page-header";
import { QueryBoundary } from "@/components/query-boundary";
import { EmptyState } from "@/components/empty-state";
import { Hint } from "@/components/hint";

import { usePortfolio, type Portfolio } from "../../hooks/use-portfolio";

// Distinct, theme-friendly hues for the allocation bar; cash is always neutral.
const SEGMENT_COLORS = [
  "var(--chart-1)",
  "var(--chart-2)",
  "var(--chart-3)",
  "var(--chart-4)",
  "var(--chart-5)",
];

export const PortfolioView = () => {
  return (
    <>
      <PageHeader
        title="Portfolio"
        description="Your positions valued at live prices."
        actions={
          <Button asChild variant="outline">
            <Link href="/markets">
              <GlobeIcon /> Find stocks
            </Link>
          </Button>
        }
      />

      <AccountSummarySection />

      <QueryBoundary fallback={<Skeleton className="h-96 w-full rounded-xl" />} errorTitle="Couldn't load your holdings">
        <Holdings />
      </QueryBoundary>
    </>
  );
};

const Holdings = () => {
  const portfolio = usePortfolio();
  const openTrade = useTradeDialog((state) => state.open);

  if (portfolio.holdings.length === 0) {
    return (
      <EmptyState
        icon={BriefcaseBusinessIcon}
        title="You don't own any stocks yet"
        description={`You have ${formatCurrency(portfolio.cash)} ready to invest. Find a company and place your first order.`}
        action={
          <Button asChild>
            <Link href="/markets">Browse markets</Link>
          </Button>
        }
      />
    );
  }

  return (
    <>
      <AllocationCard portfolio={portfolio} />

      <Card className="gap-0 overflow-hidden py-0">
        <Table>
          <TableHeader>
            <TableRow className="bg-muted/50 hover:bg-muted/50">
              <TableHead className="pl-4">Symbol</TableHead>
              <TableHead className="text-right">Shares</TableHead>
              <TableHead className="text-right">Price</TableHead>
              <TableHead className="hidden text-right md:table-cell">Avg cost</TableHead>
              <TableHead className="text-right">Market value</TableHead>
              <TableHead className="text-right">Total return</TableHead>
              <TableHead className="hidden text-right lg:table-cell">Allocation</TableHead>
              <TableHead className="pr-4 text-right">
                <span className="sr-only">Actions</span>
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {portfolio.holdings.map((holding) => (
              <TableRow key={holding.ticker}>
                <TableCell className="pl-4">
                  <Link href={`/stocks/${holding.ticker}`} className="font-semibold hover:underline">
                    {holding.ticker}
                  </Link>
                </TableCell>
                <TableCell className="text-right tabular-nums">{formatNumber(holding.quantity)}</TableCell>
                <TableCell className="text-right">
                  <LivePrice quote={holding.quote} skeletonClassName="ml-auto" />
                </TableCell>
                <TableCell className="hidden text-right tabular-nums md:table-cell">
                  {formatCurrency(holding.averageCost)}
                </TableCell>
                <TableCell className="text-right font-medium tabular-nums">
                  {formatCurrency(holding.marketValue)}
                </TableCell>
                <TableCell className="text-right">
                  <div className="flex flex-col items-end">
                    <PriceChange change={holding.unrealizedPnl} showIcon={false} />
                    <PercentChange ratio={holding.unrealizedPnlRatio} className="text-xs" />
                  </div>
                </TableCell>
                <TableCell className="hidden text-right tabular-nums lg:table-cell">
                  {formatPercent(holding.allocation, { signed: false })}
                </TableCell>
                <TableCell className="pr-4">
                  <div className="flex justify-end gap-1.5">
                    <Button size="sm" variant="outline" className="h-8" onClick={() => openTrade(holding.ticker, "buy")}>
                      Buy
                    </Button>
                    <Button size="sm" variant="outline" className="h-8" onClick={() => openTrade(holding.ticker, "sell")}>
                      Sell
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
          <TableFooter>
            <TableRow>
              <TableCell className="pl-4 font-semibold" colSpan={2}>
                Total
              </TableCell>
              <TableCell />
              <TableCell className="hidden md:table-cell" />
              <TableCell className="text-right font-semibold tabular-nums">
                {formatCurrency(portfolio.investedValue)}
              </TableCell>
              <TableCell className="text-right">
                <div className="flex flex-col items-end">
                  <PriceChange change={portfolio.unrealizedPnl} showIcon={false} />
                  <PercentChange ratio={portfolio.unrealizedPnlRatio} className="text-xs" />
                </div>
              </TableCell>
              <TableCell className="hidden lg:table-cell" />
              <TableCell />
            </TableRow>
          </TableFooter>
        </Table>
      </Card>
    </>
  );
};

const AllocationCard = ({ portfolio }: { portfolio: Portfolio }) => {
  const segments = [
    ...portfolio.holdings.map((holding, index) => ({
      label: holding.ticker,
      value: holding.marketValue,
      color: SEGMENT_COLORS[index % SEGMENT_COLORS.length],
    })),
    { label: "Cash", value: portfolio.cash, color: "var(--muted-foreground)" },
  ].filter((segment) => segment.value > 0);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Allocation</CardTitle>
        <CardDescription>How your {formatCurrency(portfolio.equity)} account is split.</CardDescription>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        <div className="flex h-3 w-full gap-0.5 overflow-hidden rounded-full" role="img" aria-label="Allocation by holding">
          {segments.map((segment) => (
            <Hint key={segment.label} label={`${segment.label}: ${formatCurrency(segment.value)}`}>
              <div
                className="h-full min-w-1 first:rounded-l-full last:rounded-r-full"
                style={{ width: `${(segment.value / portfolio.equity) * 100}%`, backgroundColor: segment.color }}
              />
            </Hint>
          ))}
        </div>
        <ul className="flex flex-wrap gap-x-5 gap-y-2 text-sm">
          {segments.map((segment) => (
            <li key={segment.label} className="flex items-center gap-1.5">
              <span className="size-2.5 rounded-full" style={{ backgroundColor: segment.color }} aria-hidden="true" />
              <span className="font-medium">{segment.label}</span>
              <span className="text-muted-foreground tabular-nums">
                {formatPercent(segment.value / portfolio.equity, { signed: false })}
              </span>
            </li>
          ))}
        </ul>
      </CardContent>
    </Card>
  );
};
