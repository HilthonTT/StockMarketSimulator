"use client";

import Link from "next/link";
import { useQuery, useSuspenseQuery } from "@tanstack/react-query";
import { ArrowLeftIcon, ClipboardListIcon, ExternalLinkIcon, ReceiptTextIcon, SearchXIcon, StarIcon } from "lucide-react";

import { useLiveQuote } from "@/modules/market/hooks/use-live-quote";
import { useWatchlist } from "@/modules/market/hooks/use-watchlist";
import type { Quote } from "@/modules/market/store";
import { LivePrice } from "@/modules/market/ui/components/live-price";
import { PriceChange } from "@/modules/market/ui/components/price-change";
import { RangedPriceChart } from "@/modules/market/ui/components/price-chart";
import { usePortfolio } from "@/modules/portfolio/hooks/use-portfolio";
import { useOpenOrders } from "@/modules/orders/hooks/use-orders";
import { OrderList } from "@/modules/orders/ui/components/order-list";
import { TradeTicket } from "@/modules/transactions/ui/components/trade-ticket";
import { TransactionList } from "@/modules/transactions/ui/components/transaction-list";

import { useTRPC } from "@/trpc/client";
import { cn, formatCurrency, formatNumber, formatPercent, ratio } from "@/lib/utils";
import { displaySymbol, isCryptoTicker } from "@/lib/assets";
import { AssetIcon } from "@/components/asset-icon";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { QueryBoundary } from "@/components/query-boundary";
import { EmptyState } from "@/components/empty-state";

import { STOCK_TRADES_PAGE_SIZE } from "../../constants";

interface StockViewProps {
  ticker: string;
}

export const StockView = ({ ticker }: StockViewProps) => {
  const { quote, isLoading, notFound } = useLiveQuote(ticker);
  const isCrypto = quote?.assetClass === "crypto" || isCryptoTicker(ticker);
  const symbol = displaySymbol(ticker);

  if (notFound) {
    return (
      <EmptyState
        icon={SearchXIcon}
        title={`We couldn't find ${ticker}`}
        description="There's no quote for this symbol. Check the ticker, or search by company name."
        action={
          <Button asChild variant="outline">
            <Link href={`/markets?tab=search&q=${encodeURIComponent(ticker)}`}>Search markets</Link>
          </Button>
        }
        className="mt-10"
      />
    );
  }

  return (
    <>
      <div className="flex flex-col gap-4">
        <Button asChild variant="ghost" size="sm" className="-ml-3 h-8 w-fit border-transparent text-muted-foreground">
          <Link href={isCrypto ? "/markets?tab=crypto" : "/markets"}>
            <ArrowLeftIcon /> Markets
          </Link>
        </Button>

        <div className="flex flex-wrap items-start justify-between gap-4">
          <div className="flex min-w-0 items-start gap-4">
            <AssetIcon ticker={ticker} assetClass={isCrypto ? "crypto" : "stock"} size="lg" />
            <div className="min-w-0">
              <div className="flex flex-wrap items-center gap-2">
                <h1 className="text-2xl font-bold tracking-tight sm:text-3xl">{quote?.name ?? symbol}</h1>
                {quote?.sector && <Badge variant="secondary">{quote.sector}</Badge>}
                {isCrypto && (
                  <Badge variant="outline" className="border-crypto/40 text-crypto">
                    24/7
                  </Badge>
                )}
              </div>
              <p className="num text-sm text-muted-foreground">{ticker}</p>
              <div className="mt-3 flex flex-wrap items-baseline gap-x-3 gap-y-1">
                <span className="num text-4xl font-bold">
                  {isLoading ? <Skeleton className="h-10 w-40" /> : <LivePrice quote={quote} />}
                </span>
                {quote && (
                  <span className="flex items-center gap-1.5 text-sm">
                    <PriceChange change={quote.change} ratio={quote.changePercent} />
                    <span className="text-muted-foreground">today</span>
                  </span>
                )}
              </div>
            </div>
          </div>
          <StockActions ticker={ticker} isCrypto={isCrypto} />
        </div>
      </div>

      <KeyStats quote={quote} isCrypto={isCrypto} />

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="flex min-w-0 flex-col gap-6 lg:col-span-2">
          <Card>
            <CardContent>
              <QueryBoundary fallback={<Skeleton className="h-[380px] w-full rounded-lg" />}>
                <ChartWithCost ticker={ticker} previousClose={quote?.previousClose} />
              </QueryBoundary>
            </CardContent>
          </Card>

          <QueryBoundary fallback={<Skeleton className="h-40 w-full rounded-xl" />} errorTitle="Couldn't load your position">
            <PositionCard ticker={ticker} isCrypto={isCrypto} />
          </QueryBoundary>

          <TickerOrders ticker={ticker} />

          <Card className="gap-2">
            <CardHeader>
              <CardTitle>Your {symbol} trades</CardTitle>
            </CardHeader>
            <CardContent>
              <QueryBoundary fallback={<Skeleton className="h-32 w-full" />} errorTitle="Couldn't load your trades">
                <TickerTrades ticker={ticker} />
              </QueryBoundary>
            </CardContent>
          </Card>
        </div>

        <div className="min-w-0">
          <Card className="lg:sticky lg:top-20">
            <CardHeader>
              <CardTitle>Trade {symbol}</CardTitle>
              <CardDescription>Market, limit or stop order with your virtual cash.</CardDescription>
            </CardHeader>
            <CardContent>
              <TradeTicket ticker={ticker} />
            </CardContent>
          </Card>
        </div>
      </div>
    </>
  );
};

/** Open, previous close and where the price sits in today's range. */
const KeyStats = ({ quote, isCrypto }: { quote: Quote | undefined; isCrypto: boolean }) => {
  if (!quote) {
    return <Skeleton className="h-20 w-full rounded-xl" />;
  }

  const span = quote.dayHigh - quote.dayLow;
  const position = span > 0 ? ((quote.price - quote.dayLow) / span) * 100 : 50;

  const stats = [
    { label: "Open", value: formatCurrency(quote.open) },
    { label: "Prev close", value: formatCurrency(quote.previousClose) },
    { label: "Day low", value: formatCurrency(quote.dayLow) },
    { label: "Day high", value: formatCurrency(quote.dayHigh) },
  ];

  return (
    <Card className="gap-0 py-0">
      <CardContent className="grid gap-4 p-4 sm:grid-cols-2 lg:grid-cols-[repeat(4,minmax(0,1fr))_minmax(0,2fr)]">
        {stats.map((stat) => (
          <div key={stat.label}>
            <p className="text-xs uppercase tracking-wide text-muted-foreground">{stat.label}</p>
            <p className="num text-base font-semibold">{stat.value}</p>
          </div>
        ))}
        <div className="sm:col-span-2 lg:col-span-1">
          <p className="text-xs uppercase tracking-wide text-muted-foreground">
            Day range {isCrypto ? "(24/7 market)" : ""}
          </p>
          <div
            className="relative mt-2.5 h-1.5 rounded-full bg-gradient-to-r from-loss/60 via-muted-foreground/30 to-gain/60"
            role="img"
            aria-label={`Price is ${Math.round(position)}% of the way from today's low to today's high`}
          >
            <span
              className="absolute top-1/2 size-3 -translate-x-1/2 -translate-y-1/2 rounded-full border-2 border-background bg-foreground shadow"
              style={{ left: `${Math.min(100, Math.max(0, position))}%` }}
            />
          </div>
          <div className="num mt-1.5 flex justify-between text-xs text-muted-foreground">
            <span>{formatCurrency(quote.dayLow)}</span>
            <span>{formatCurrency(quote.dayHigh)}</span>
          </div>
        </div>
      </CardContent>
    </Card>
  );
};

const StockActions = ({ ticker, isCrypto }: { ticker: string; isCrypto: boolean }) => {
  const trpc = useTRPC();
  const { tickers, toggle } = useWatchlist();
  const isWatched = tickers.includes(ticker);

  // Optional: the API verifies the external page exists, so don't block on it.
  const { data: link } = useQuery(
    trpc.shorten.getShortenUrl.queryOptions({ ticker }, { retry: false, staleTime: Infinity, enabled: !isCrypto })
  );
  const infoUrl = link?.shortCode || `https://finance.yahoo.com/quote/${encodeURIComponent(ticker)}`;

  return (
    <div className="flex gap-2">
      <Button variant="outline" onClick={() => toggle(ticker)} aria-pressed={isWatched}>
        <StarIcon className={cn(isWatched && "fill-amber-400 text-amber-400")} />
        {isWatched ? "Watching" : "Watch"}
      </Button>
      <Button asChild variant="outline">
        <a href={infoUrl} target="_blank" rel="noopener noreferrer">
          <ExternalLinkIcon /> {isCrypto ? "Coin info" : "Company info"}
        </a>
      </Button>
    </div>
  );
};

const ChartWithCost = ({ ticker, previousClose }: { ticker: string; previousClose?: number }) => {
  const { holdings } = usePortfolio();
  const holding = holdings.find((h) => h.ticker === ticker);

  return (
    <RangedPriceChart
      ticker={ticker}
      previousClose={previousClose}
      references={holding ? [{ value: holding.averageCost, label: "Avg cost" }] : []}
    />
  );
};

const PositionCard = ({ ticker, isCrypto }: { ticker: string; isCrypto: boolean }) => {
  const portfolio = usePortfolio();
  const holding = portfolio.holdings.find((h) => h.ticker === ticker);
  const symbol = displaySymbol(ticker);

  if (!holding) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Your position</CardTitle>
          <CardDescription>
            You don&apos;t own {symbol}. With {formatCurrency(portfolio.cash)} in cash you can open a position from the
            order ticket.
          </CardDescription>
        </CardHeader>
      </Card>
    );
  }

  const stats = [
    { label: isCrypto ? "Coins" : "Shares", value: formatNumber(holding.quantity) },
    { label: "Average cost", value: formatCurrency(holding.averageCost) },
    { label: "Market value", value: formatCurrency(holding.marketValue) },
    {
      label: "Today",
      value: <PriceChange change={holding.dayPnl} showIcon={false} />,
    },
    {
      label: "Unrealized P&L",
      value: <PriceChange change={holding.unrealizedPnl} ratio={holding.unrealizedPnlRatio} showIcon={false} />,
    },
    {
      label: "Portfolio share",
      value: formatPercent(ratio(holding.marketValue, portfolio.equity), { signed: false }),
    },
  ];

  return (
    <Card>
      <CardHeader>
        <CardTitle>Your position</CardTitle>
      </CardHeader>
      <CardContent>
        <dl className="grid grid-cols-2 gap-x-6 gap-y-4 sm:grid-cols-3">
          {stats.map((stat) => (
            <div key={stat.label}>
              <dt className="text-sm text-muted-foreground">{stat.label}</dt>
              <dd className="num text-lg font-semibold">{stat.value}</dd>
            </div>
          ))}
        </dl>
      </CardContent>
    </Card>
  );
};

const TickerOrders = ({ ticker }: { ticker: string }) => {
  const { data } = useOpenOrders();
  const orders = (data ?? []).filter((order) => order.ticker === ticker);

  if (orders.length === 0) {
    return null;
  }

  return (
    <Card className="gap-2 overflow-hidden pb-0">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <ClipboardListIcon className="size-4 text-muted-foreground" aria-hidden="true" />
          Open orders
        </CardTitle>
        <CardDescription>Waiting for the live price to reach their trigger.</CardDescription>
      </CardHeader>
      <OrderList orders={orders} hideTicker />
    </Card>
  );
};

const TickerTrades = ({ ticker }: { ticker: string }) => {
  const trpc = useTRPC();
  const { data } = useSuspenseQuery(
    trpc.transactions.getMany.queryOptions({ searchTerm: ticker, pageSize: STOCK_TRADES_PAGE_SIZE })
  );

  // The API search is a text match, so keep only exact ticker matches.
  const trades = data.data.filter((transaction) => transaction.ticker.toUpperCase() === ticker);
  const symbol = displaySymbol(ticker);

  if (trades.length === 0) {
    return (
      <EmptyState
        icon={ReceiptTextIcon}
        title={`No ${symbol} trades yet`}
        description="Orders you fill for this asset will be listed here."
        className="py-8"
      />
    );
  }

  return (
    <>
      <TransactionList transactions={trades} />
      <Button asChild variant="link" className="px-0">
        <Link href={`/transactions?search=${encodeURIComponent(ticker)}`}>See all {symbol} history</Link>
      </Button>
    </>
  );
};
