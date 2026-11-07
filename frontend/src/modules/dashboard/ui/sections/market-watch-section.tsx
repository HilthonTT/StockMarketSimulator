"use client";

import Link from "next/link";
import { useState } from "react";
import { useSuspenseQuery } from "@tanstack/react-query";
import { CompassIcon, GlobeIcon } from "lucide-react";

import { usePortfolio } from "@/modules/portfolio/hooks/use-portfolio";
import { useLiveQuote } from "@/modules/market/hooks/use-live-quote";
import { LivePrice } from "@/modules/market/ui/components/live-price";
import { PriceChange } from "@/modules/market/ui/components/price-change";
import { PriceChart } from "@/modules/market/ui/components/price-chart";
import { useTradeDialog } from "@/modules/transactions/hooks/use-trade-dialog";

import { useTRPC } from "@/trpc/client";
import { formatCurrency, ratio } from "@/lib/utils";
import { STARTING_CASH } from "@/constants";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";
import { QueryBoundary } from "@/components/query-boundary";
import { EmptyState } from "@/components/empty-state";

export const MarketWatchSection = () => (
  <QueryBoundary
    errorTitle="Couldn't load the market watch"
    fallback={<Skeleton className="h-[460px] w-full rounded-xl" />}
  >
    <MarketWatch />
  </QueryBoundary>
);

const MarketWatch = () => {
  const portfolio = usePortfolio();
  const tickers = portfolio.holdings.map((holding) => holding.ticker);

  const [selected, setSelected] = useState<string | undefined>(undefined);
  // Fall back to the largest holding when nothing (or a since-sold ticker) is selected.
  const ticker = selected && tickers.includes(selected) ? selected : tickers[0];

  if (!ticker) {
    return <GetStarted />;
  }

  const holding = portfolio.holdings.find((h) => h.ticker === ticker);

  return (
    <Card>
      <CardHeader className="gap-4">
        {tickers.length > 1 && (
          <ToggleGroup
            type="single"
            variant="outline"
            size="sm"
            value={ticker}
            onValueChange={(value) => value && setSelected(value)}
            className="flex-wrap justify-start"
            aria-label="Choose a holding to chart"
          >
            {tickers.slice(0, 8).map((t) => (
              <ToggleGroupItem key={t} value={t} className="px-3 font-semibold">
                {t}
              </ToggleGroupItem>
            ))}
          </ToggleGroup>
        )}
        <WatchHeader ticker={ticker} averageCost={holding?.averageCost} />
      </CardHeader>
      <CardContent>
        <WatchChart ticker={ticker} averageCost={holding?.averageCost} />
      </CardContent>
    </Card>
  );
};

const WatchHeader = ({ ticker, averageCost }: { ticker: string; averageCost?: number }) => {
  const { quote } = useLiveQuote(ticker);
  const openTrade = useTradeDialog((state) => state.open);

  const sessionChange = quote ? quote.price - quote.sessionOpen : 0;

  return (
    <div className="flex flex-wrap items-end justify-between gap-4">
      <div>
        <Link href={`/stocks/${ticker}`} className="text-sm font-semibold text-muted-foreground hover:underline">
          {ticker}
        </Link>
        <div className="text-3xl font-bold">
          <LivePrice quote={quote} skeletonClassName="h-9 w-32" />
        </div>
        <div className="flex flex-wrap items-center gap-x-3 gap-y-1 text-sm">
          <span className="inline-flex items-center gap-1">
            <PriceChange change={sessionChange} ratio={quote ? ratio(sessionChange, quote.sessionOpen) : 0} />
            <span className="text-muted-foreground">this session</span>
          </span>
          {averageCost !== undefined && (
            <span className="text-muted-foreground">Avg cost {formatCurrency(averageCost)}</span>
          )}
        </div>
      </div>
      <div className="flex gap-2">
        <Button size="sm" className="bg-gain text-white border-transparent hover:bg-gain/90" onClick={() => openTrade(ticker, "buy")}>
          Buy
        </Button>
        <Button size="sm" variant="outline" onClick={() => openTrade(ticker, "sell")}>
          Sell
        </Button>
      </div>
    </div>
  );
};

const WatchChart = ({ ticker, averageCost }: { ticker: string; averageCost?: number }) => {
  const { quote } = useLiveQuote(ticker);

  return (
    <PriceChart
      history={quote?.history}
      baseline={averageCost ? { value: averageCost, label: "Your avg cost" } : undefined}
    />
  );
};

/** First-run card: explains the game and suggests a few stocks to start with. */
const GetStarted = () => {
  const trpc = useTRPC();
  const { data } = useSuspenseQuery(trpc.stocks.search.queryOptions({ pageSize: 6 }));
  const openTrade = useTradeDialog((state) => state.open);

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-xl">Make your first trade</CardTitle>
        <CardDescription>
          You have {formatCurrency(STARTING_CASH)} in virtual cash. Prices move every second, so buy low, sell high
          and watch your account value change live.
        </CardDescription>
      </CardHeader>
      <CardContent>
        {data.items.length > 0 ? (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {data.items.map((stock) => (
              <div key={stock.ticker} className="flex items-center justify-between gap-3 rounded-lg border p-3">
                <Link href={`/stocks/${stock.ticker.toUpperCase()}`} className="min-w-0 hover:underline">
                  <p className="font-semibold">{stock.ticker.toUpperCase()}</p>
                  <p className="truncate text-xs text-muted-foreground">{stock.name}</p>
                </Link>
                <Button size="sm" variant="outline" onClick={() => openTrade(stock.ticker)}>
                  Buy
                </Button>
              </div>
            ))}
          </div>
        ) : (
          <EmptyState
            icon={CompassIcon}
            title="Find a company to invest in"
            description="Search by ticker or company name to see its live price."
            action={
              <Button asChild variant="outline">
                <Link href="/markets">
                  <GlobeIcon /> Browse markets
                </Link>
              </Button>
            }
          />
        )}
      </CardContent>
    </Card>
  );
};
