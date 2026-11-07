"use client";

import Link from "next/link";
import { useQuery, useSuspenseQuery } from "@tanstack/react-query";
import { ArrowLeftIcon, ExternalLinkIcon, ReceiptTextIcon, SearchXIcon, StarIcon } from "lucide-react";

import { useLiveQuote } from "@/modules/market/hooks/use-live-quote";
import { useWatchlist } from "@/modules/market/hooks/use-watchlist";
import { LivePrice } from "@/modules/market/ui/components/live-price";
import { PriceChange } from "@/modules/market/ui/components/price-change";
import { PriceChart } from "@/modules/market/ui/components/price-chart";
import { usePortfolio } from "@/modules/portfolio/hooks/use-portfolio";
import { TradeTicket } from "@/modules/transactions/ui/components/trade-ticket";
import { TransactionList } from "@/modules/transactions/ui/components/transaction-list";

import { useTRPC } from "@/trpc/client";
import { cn, formatCurrency, formatNumber, formatPercent, ratio } from "@/lib/utils";
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

  if (notFound) {
    return (
      <EmptyState
        icon={SearchXIcon}
        title={`We couldn't find ${ticker}`}
        description="There's no quote for this symbol. Check the ticker, or search by company name."
        action={
          <Button asChild variant="outline">
            <Link href={`/markets?q=${encodeURIComponent(ticker)}`}>Search markets</Link>
          </Button>
        }
        className="mt-10"
      />
    );
  }

  const sessionChange = quote ? quote.price - quote.sessionOpen : 0;

  return (
    <>
      <div className="flex flex-col gap-4">
        <Button asChild variant="ghost" size="sm" className="-ml-3 h-8 w-fit border-transparent text-muted-foreground">
          <Link href="/markets">
            <ArrowLeftIcon /> Markets
          </Link>
        </Button>

        <div className="flex flex-wrap items-start justify-between gap-4">
          <div className="min-w-0">
            <StockTitle ticker={ticker} />
            <div className="mt-2 text-4xl font-bold">
              {isLoading ? <Skeleton className="h-10 w-40" /> : <LivePrice quote={quote} />}
            </div>
            <div className="mt-1 flex items-center gap-1.5 text-sm">
              <PriceChange change={sessionChange} ratio={quote ? ratio(sessionChange, quote.sessionOpen) : 0} />
              <span className="text-muted-foreground">since you opened the app</span>
            </div>
          </div>
          <StockActions ticker={ticker} />
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="flex min-w-0 flex-col gap-6 lg:col-span-2">
          <Card>
            <CardHeader>
              <CardTitle>Live price</CardTitle>
              <CardDescription>Simulated ticks, roughly one per second.</CardDescription>
            </CardHeader>
            <CardContent>
              <QueryBoundary fallback={<PriceChart history={undefined} />}>
                <ChartWithCost ticker={ticker} history={quote?.history} />
              </QueryBoundary>
            </CardContent>
          </Card>

          <QueryBoundary fallback={<Skeleton className="h-40 w-full rounded-xl" />} errorTitle="Couldn't load your position">
            <PositionCard ticker={ticker} />
          </QueryBoundary>

          <Card className="gap-2">
            <CardHeader>
              <CardTitle>Your {ticker} trades</CardTitle>
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
              <CardTitle>Trade {ticker}</CardTitle>
              <CardDescription>Market order with your virtual cash.</CardDescription>
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

const StockTitle = ({ ticker }: { ticker: string }) => {
  const trpc = useTRPC();
  const { data } = useQuery(trpc.stocks.search.queryOptions({ searchTerm: ticker, pageSize: 5 }));
  const company = data?.items.find((item) => item.ticker.toUpperCase() === ticker);

  return (
    <div>
      <h1 className="text-2xl font-bold tracking-tight sm:text-3xl">{ticker}</h1>
      <p className="truncate text-muted-foreground">
        {company ? `${company.name} · ${company.region} · ${company.currency}` : " "}
      </p>
    </div>
  );
};

const StockActions = ({ ticker }: { ticker: string }) => {
  const trpc = useTRPC();
  const { tickers, toggle } = useWatchlist();
  const isWatched = tickers.includes(ticker);

  // Optional: the API verifies the external page exists, so don't block on it.
  const { data: link } = useQuery(
    trpc.shorten.getShortenUrl.queryOptions({ ticker }, { retry: false, staleTime: Infinity })
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
          <ExternalLinkIcon /> Company info
        </a>
      </Button>
    </div>
  );
};

const ChartWithCost = ({ ticker, history }: { ticker: string; history: Parameters<typeof PriceChart>[0]["history"] }) => {
  const { holdings } = usePortfolio();
  const holding = holdings.find((h) => h.ticker === ticker);

  return (
    <PriceChart
      history={history}
      baseline={holding ? { value: holding.averageCost, label: "Your avg cost" } : undefined}
    />
  );
};

const PositionCard = ({ ticker }: { ticker: string }) => {
  const portfolio = usePortfolio();
  const holding = portfolio.holdings.find((h) => h.ticker === ticker);

  if (!holding) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Your position</CardTitle>
          <CardDescription>
            You don&apos;t own {ticker}. With {formatCurrency(portfolio.cash)} in cash you can start a position from the
            order ticket.
          </CardDescription>
        </CardHeader>
      </Card>
    );
  }

  const stats = [
    { label: "Shares", value: formatNumber(holding.quantity) },
    { label: "Average cost", value: formatCurrency(holding.averageCost) },
    { label: "Market value", value: formatCurrency(holding.marketValue) },
    { label: "Cost basis", value: formatCurrency(holding.totalCost) },
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
              <dd className="text-lg font-semibold tabular-nums">{stat.value}</dd>
            </div>
          ))}
        </dl>
      </CardContent>
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

  if (trades.length === 0) {
    return (
      <EmptyState
        icon={ReceiptTextIcon}
        title={`No ${ticker} trades yet`}
        description="Orders you place for this stock will be listed here."
        className="py-8"
      />
    );
  }

  return (
    <>
      <TransactionList transactions={trades} />
      <Button asChild variant="link" className="px-0">
        <Link href={`/transactions?search=${encodeURIComponent(ticker)}`}>See all {ticker} history</Link>
      </Button>
    </>
  );
};
