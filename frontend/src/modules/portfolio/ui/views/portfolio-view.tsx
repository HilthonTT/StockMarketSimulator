"use client";

import Link from "next/link";
import { BriefcaseBusinessIcon, GlobeIcon } from "lucide-react";

import { AccountSummarySection } from "@/modules/dashboard/ui/sections/account-summary-section";
import { LivePrice } from "@/modules/market/ui/components/live-price";
import { PercentChange, PriceChange } from "@/modules/market/ui/components/price-change";
import { QuoteSparkline } from "@/modules/market/ui/components/market-widgets";
import { useTradeDialog } from "@/modules/transactions/hooks/use-trade-dialog";

import { formatCurrency, formatNumber, formatPercent } from "@/lib/utils";
import { assetHref, displaySymbol } from "@/lib/assets";
import { AssetIcon } from "@/components/asset-icon";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableFooter, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { PageHeader } from "@/components/page-header";
import { QueryBoundary } from "@/components/query-boundary";
import { EmptyState } from "@/components/empty-state";

import { usePortfolio } from "../../hooks/use-portfolio";
import { AccountValueChart } from "../components/account-value-chart";
import { AllocationDonut } from "../components/allocation-donut";

export const PortfolioView = () => {
  return (
    <>
      <PageHeader
        title="Portfolio"
        description="Your stocks and crypto, valued at live prices."
        actions={
          <Button asChild variant="outline">
            <Link href="/markets">
              <GlobeIcon /> Find assets
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
        title="You don't own anything yet"
        description={`You have ${formatCurrency(portfolio.cash)} ready to invest. Find a stock or coin and place your first order.`}
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
      <div className="grid gap-6 lg:grid-cols-3">
        <div className="min-w-0 lg:col-span-2">
          <AccountValueChart portfolio={portfolio} height={300} />
        </div>
        <AllocationDonut portfolio={portfolio} />
      </div>

      <Card className="gap-0 overflow-hidden py-0">
        <Table>
          <TableHeader>
            <TableRow className="bg-muted/50 hover:bg-muted/50">
              <TableHead className="pl-4">Asset</TableHead>
              <TableHead className="text-right">Qty</TableHead>
              <TableHead className="text-right">Price</TableHead>
              <TableHead className="hidden text-right sm:table-cell">Today</TableHead>
              <TableHead className="hidden xl:table-cell">
                <span className="sr-only">Today&apos;s trend</span>
              </TableHead>
              <TableHead className="hidden text-right md:table-cell">Avg cost</TableHead>
              <TableHead className="text-right">Value</TableHead>
              <TableHead className="text-right">Total return</TableHead>
              <TableHead className="hidden text-right lg:table-cell">Weight</TableHead>
              <TableHead className="pr-4 text-right">
                <span className="sr-only">Actions</span>
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {portfolio.holdings.map((holding) => (
              <TableRow key={holding.ticker}>
                <TableCell className="pl-4">
                  <Link href={assetHref(holding.ticker)} className="group flex items-center gap-3">
                    <AssetIcon ticker={holding.ticker} assetClass={holding.quote?.assetClass} size="sm" />
                    <div className="min-w-0">
                      <p className="font-semibold group-hover:underline">{displaySymbol(holding.ticker)}</p>
                      <p className="max-w-[10rem] truncate text-xs text-muted-foreground">
                        {holding.quote?.name ?? holding.ticker}
                      </p>
                    </div>
                  </Link>
                </TableCell>
                <TableCell className="num text-right">{formatNumber(holding.quantity)}</TableCell>
                <TableCell className="text-right">
                  <LivePrice quote={holding.quote} className="num" skeletonClassName="ml-auto" />
                </TableCell>
                <TableCell className="hidden text-right sm:table-cell">
                  <div className="flex flex-col items-end">
                    <PriceChange change={holding.dayPnl} showIcon={false} className="num" />
                    <PercentChange ratio={holding.quote?.changePercent ?? 0} className="num text-xs" />
                  </div>
                </TableCell>
                <TableCell className="hidden xl:table-cell">
                  {holding.quote && <QuoteSparkline quote={holding.quote} />}
                </TableCell>
                <TableCell className="num hidden text-right md:table-cell">{formatCurrency(holding.averageCost)}</TableCell>
                <TableCell className="num text-right font-medium">{formatCurrency(holding.marketValue)}</TableCell>
                <TableCell className="text-right">
                  <div className="flex flex-col items-end">
                    <PriceChange change={holding.unrealizedPnl} showIcon={false} className="num" />
                    <PercentChange ratio={holding.unrealizedPnlRatio} className="num text-xs" />
                  </div>
                </TableCell>
                <TableCell className="num hidden text-right lg:table-cell">
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
              <TableCell className="pl-4 font-semibold" colSpan={3}>
                Total
              </TableCell>
              <TableCell className="hidden text-right sm:table-cell">
                <PriceChange change={portfolio.dayPnl} showIcon={false} className="num" />
              </TableCell>
              <TableCell className="hidden xl:table-cell" />
              <TableCell className="hidden md:table-cell" />
              <TableCell className="num text-right font-semibold">{formatCurrency(portfolio.investedValue)}</TableCell>
              <TableCell className="text-right">
                <div className="flex flex-col items-end">
                  <PriceChange change={portfolio.unrealizedPnl} showIcon={false} className="num" />
                  <PercentChange ratio={portfolio.unrealizedPnlRatio} className="num text-xs" />
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
