"use client";

import Link from "next/link";
import { useState } from "react";
import { useQueryStates } from "nuqs";
import { useSuspenseInfiniteQuery } from "@tanstack/react-query";
import { ArrowRightIcon, SearchIcon, SearchXIcon, StarIcon, XIcon } from "lucide-react";

import { useTradeDialog } from "@/modules/transactions/hooks/use-trade-dialog";
import { useWatchlist } from "@/modules/market/hooks/use-watchlist";

import { useTRPC } from "@/trpc/client";
import { cn } from "@/lib/utils";
import { useDebounce } from "@/hooks/use-debounce";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { PageHeader } from "@/components/page-header";
import { QueryBoundary } from "@/components/query-boundary";
import { EmptyState } from "@/components/empty-state";
import { InfiniteScroll } from "@/components/infinite-scroll";

import { MARKETS_PAGE_SIZE } from "../../constants";
import { marketsSearchParams } from "../../search-params";

export const MarketsView = () => {
  const [{ q }, setParams] = useQueryStates(marketsSearchParams);
  const [value, setValue] = useState(q);
  const searchTerm = useDebounce(value.trim(), 300);

  const updateValue = (next: string) => {
    setValue(next);
    void setParams({ q: next.trim() });
  };

  return (
    <>
      <PageHeader
        title="Markets"
        description="Search companies by ticker or name, then open a stock to see its live price and trade it."
      />

      <div className="relative max-w-xl">
        <SearchIcon className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" aria-hidden="true" />
        <Input
          value={value}
          onChange={(event) => updateValue(event.target.value)}
          placeholder="Search e.g. AAPL, Tesla, Microsoft"
          className="h-11 pl-9 pr-10"
          aria-label="Search stocks"
          autoFocus
        />
        {value && (
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="absolute right-1.5 top-1/2 size-8 -translate-y-1/2 border-transparent"
            onClick={() => updateValue("")}
            aria-label="Clear search"
          >
            <XIcon />
          </Button>
        )}
      </div>

      <QueryBoundary fallback={<ResultsSkeleton />} errorTitle="Search failed">
        <MarketResults searchTerm={searchTerm} />
      </QueryBoundary>
    </>
  );
};

const ResultsSkeleton = () => (
  <Card className="p-4 gap-3">
    {Array.from({ length: 8 }, (_, index) => (
      <Skeleton key={index} className="h-10 w-full" />
    ))}
  </Card>
);

const TICKER_PATTERN = /^[A-Za-z][A-Za-z0-9.\-]{0,9}$/;

const MarketResults = ({ searchTerm }: { searchTerm: string }) => {
  const trpc = useTRPC();
  const openTrade = useTradeDialog((state) => state.open);
  const { tickers: watchlist, toggle } = useWatchlist();

  const { data, hasNextPage, fetchNextPage, isFetchingNextPage, isFetching } = useSuspenseInfiniteQuery(
    trpc.stocks.search.infiniteQueryOptions(
      { searchTerm: searchTerm || undefined, pageSize: MARKETS_PAGE_SIZE },
      { getNextPageParam: (lastPage) => (lastPage.hasNextPage ? lastPage.page + 1 : undefined) }
    )
  );

  const stocks = data.pages.flatMap((page) => page.items);

  if (stocks.length === 0) {
    const upper = searchTerm.toUpperCase();

    return (
      <EmptyState
        icon={SearchXIcon}
        title={searchTerm ? `No companies match "${searchTerm}"` : "No stocks available yet"}
        description={
          searchTerm
            ? "Try the exact ticker symbol, or a different spelling of the company name."
            : "Search for a company to add it to the market list."
        }
        action={
          TICKER_PATTERN.test(searchTerm) ? (
            <Button asChild variant="outline">
              <Link href={`/stocks/${upper}`}>
                Try opening {upper} directly <ArrowRightIcon />
              </Link>
            </Button>
          ) : undefined
        }
      />
    );
  }

  return (
    <Card className={cn("py-0 gap-0 overflow-hidden transition-opacity", isFetching && !isFetchingNextPage && "opacity-60")}>
      <Table>
        <TableHeader>
          <TableRow className="bg-muted/50 hover:bg-muted/50">
            <TableHead className="w-10 pl-4"><span className="sr-only">Watch</span></TableHead>
            <TableHead>Symbol</TableHead>
            <TableHead>Company</TableHead>
            <TableHead className="hidden md:table-cell">Type</TableHead>
            <TableHead className="hidden lg:table-cell">Region</TableHead>
            <TableHead className="hidden sm:table-cell">Currency</TableHead>
            <TableHead className="pr-4 text-right">Action</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {stocks.map((stock) => {
            const ticker = stock.ticker.toUpperCase();
            const isWatched = watchlist.includes(ticker);

            return (
              <TableRow key={ticker}>
                <TableCell className="pl-4">
                  <Button
                    variant="ghost"
                    size="icon"
                    className="size-8 border-transparent"
                    onClick={() => toggle(ticker)}
                    aria-pressed={isWatched}
                    aria-label={isWatched ? `Remove ${ticker} from watchlist` : `Add ${ticker} to watchlist`}
                  >
                    <StarIcon className={cn(isWatched && "fill-amber-400 text-amber-400")} />
                  </Button>
                </TableCell>
                <TableCell>
                  <Link href={`/stocks/${ticker}`} className="font-semibold hover:underline">
                    {ticker}
                  </Link>
                </TableCell>
                <TableCell className="max-w-[16rem] truncate">
                  <Link href={`/stocks/${ticker}`} className="hover:underline">
                    {stock.name}
                  </Link>
                </TableCell>
                <TableCell className="hidden md:table-cell text-muted-foreground">{stock.type}</TableCell>
                <TableCell className="hidden lg:table-cell text-muted-foreground">{stock.region}</TableCell>
                <TableCell className="hidden sm:table-cell text-muted-foreground">{stock.currency}</TableCell>
                <TableCell className="pr-4 text-right">
                  <Button size="sm" variant="outline" className="h-8" onClick={() => openTrade(ticker)}>
                    Trade
                  </Button>
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
      <InfiniteScroll
        hasNextPage={hasNextPage}
        fetchNextPage={fetchNextPage}
        isFetchingNextPage={isFetchingNextPage}
      />
    </Card>
  );
};
