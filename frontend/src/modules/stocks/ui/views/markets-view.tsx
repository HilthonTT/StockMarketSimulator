"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import { useQueryStates } from "nuqs";
import { useSuspenseInfiniteQuery } from "@tanstack/react-query";
import { ArrowRightIcon, BitcoinIcon, CandlestickChartIcon, SearchIcon, SearchXIcon, StarIcon, XIcon } from "lucide-react";

import { useTradeDialog } from "@/modules/transactions/hooks/use-trade-dialog";
import { useWatchlist } from "@/modules/market/hooks/use-watchlist";
import { useMarket } from "@/modules/market/hooks/use-market";
import type { Quote } from "@/modules/market/store";
import { PercentChange, PriceChange } from "@/modules/market/ui/components/price-change";
import { IndexCard, MoversCard, QuotePrice, QuoteSparkline } from "@/modules/market/ui/components/market-widgets";

import { useTRPC } from "@/trpc/client";
import { cn, formatCurrency } from "@/lib/utils";
import { assetHref, displaySymbol } from "@/lib/assets";
import { useDebounce } from "@/hooks/use-debounce";
import { AssetIcon } from "@/components/asset-icon";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { PageHeader } from "@/components/page-header";
import { QueryBoundary } from "@/components/query-boundary";
import { EmptyState } from "@/components/empty-state";
import { InfiniteScroll } from "@/components/infinite-scroll";

import { MARKETS_PAGE_SIZE } from "../../constants";
import { marketsSearchParams, type MarketTab } from "../../search-params";

export const MarketsView = () => {
  const [{ tab }, setParams] = useQueryStates(marketsSearchParams);

  return (
    <>
      <PageHeader
        title="Markets"
        description="Every asset in the simulator ticks live. Stocks follow a simulated trading day; crypto trades 24/7."
      />

      <MarketOverview />

      <Tabs value={tab} onValueChange={(value) => void setParams({ tab: value as MarketTab, sector: "" })} className="gap-4">
        <TabsList>
          <TabsTrigger value="stocks">
            <CandlestickChartIcon aria-hidden="true" /> Stocks
          </TabsTrigger>
          <TabsTrigger value="crypto">
            <BitcoinIcon aria-hidden="true" /> Crypto
          </TabsTrigger>
          <TabsTrigger value="search">
            <SearchIcon aria-hidden="true" /> All symbols
          </TabsTrigger>
        </TabsList>
        <TabsContent value="stocks">
          <UniverseTable assetClass="stock" />
        </TabsContent>
        <TabsContent value="crypto">
          <UniverseTable assetClass="crypto" />
        </TabsContent>
        <TabsContent value="search" className="flex flex-col gap-4">
          <SymbolSearch />
        </TabsContent>
      </Tabs>
    </>
  );
};

const MarketOverview = () => {
  const { market, isLoading } = useMarket();

  if (isLoading) {
    return (
      <div className="grid gap-4 lg:grid-cols-3">
        {Array.from({ length: 3 }, (_, index) => (
          <Skeleton key={index} className="h-56 w-full rounded-xl" />
        ))}
      </div>
    );
  }

  if (!market) {
    return null;
  }

  const breadth = {
    up: market.stocks.filter((quote) => quote.change > 0).length,
    down: market.stocks.filter((quote) => quote.change < 0).length,
  };

  return (
    <div className="grid gap-4 lg:grid-cols-3">
      <IndexCard index={market.index} breadth={breadth} />
      <MoversCard title="Top gainers" quotes={market.gainers} direction="up" />
      <MoversCard title="Top losers" quotes={market.losers} direction="down" />
    </div>
  );
};

type SortKey = "ticker" | "price" | "changePercent";

const UniverseTable = ({ assetClass }: { assetClass: Quote["assetClass"] }) => {
  const { market, isLoading, isError } = useMarket();
  const [{ sector }, setParams] = useQueryStates(marketsSearchParams);
  const [sort, setSort] = useState<{ key: SortKey; desc: boolean }>({ key: "ticker", desc: false });
  const openTrade = useTradeDialog((state) => state.open);
  const { tickers: watchlist, toggle } = useWatchlist();

  const quotes = useMemo(() => (assetClass === "crypto" ? market?.crypto : market?.stocks) ?? [], [market, assetClass]);
  const sectors = useMemo(
    () => [...new Set(quotes.map((quote) => quote.sector).filter((value): value is string => !!value))].sort(),
    [quotes]
  );

  const rows = useMemo(() => {
    const filtered = sector ? quotes.filter((quote) => quote.sector === sector) : quotes;
    const direction = sort.desc ? -1 : 1;

    return [...filtered].sort((a, b) =>
      sort.key === "ticker" ? a.ticker.localeCompare(b.ticker) * direction : (a[sort.key] - b[sort.key]) * direction
    );
  }, [quotes, sector, sort]);

  if (isLoading) {
    return <Skeleton className="h-[480px] w-full rounded-xl" />;
  }

  if (isError || !market) {
    return (
      <EmptyState
        icon={SearchXIcon}
        title="The market overview isn't available"
        description="The trading server didn't return the simulated universe. You can still search for symbols."
      />
    );
  }

  const sortButton = (key: SortKey, label: string, align: "left" | "right" = "right") => (
    <button
      type="button"
      onClick={() => setSort((current) => ({ key, desc: current.key === key ? !current.desc : key !== "ticker" }))}
      className={cn("inline-flex cursor-pointer items-center gap-1 hover:text-foreground", align === "right" && "flex-row-reverse")}
      aria-label={`Sort by ${label}`}
    >
      {label}
      {sort.key === key && <span aria-hidden="true">{sort.desc ? "↓" : "↑"}</span>}
    </button>
  );

  return (
    <div className="flex flex-col gap-3">
      {assetClass === "stock" && sectors.length > 1 && (
        <div className="flex flex-wrap gap-1.5" role="group" aria-label="Filter by sector">
          {["", ...sectors].map((option) => (
            <Button
              key={option || "all"}
              size="sm"
              variant={sector === option ? "default" : "outline"}
              className="h-7 rounded-full px-3 text-xs"
              aria-pressed={sector === option}
              onClick={() => void setParams({ sector: option })}
            >
              {option || "All sectors"}
            </Button>
          ))}
        </div>
      )}

      <Card className="gap-0 overflow-hidden py-0">
        <Table>
          <TableHeader>
            <TableRow className="bg-muted/50 hover:bg-muted/50">
              <TableHead className="w-10 pl-4">
                <span className="sr-only">Watch</span>
              </TableHead>
              <TableHead>{sortButton("ticker", "Asset", "left")}</TableHead>
              <TableHead className="hidden md:table-cell">{assetClass === "crypto" ? "Market" : "Sector"}</TableHead>
              <TableHead className="text-right">{sortButton("price", "Price")}</TableHead>
              <TableHead className="text-right">{sortButton("changePercent", "Day")}</TableHead>
              <TableHead className="hidden text-right lg:table-cell">Day range</TableHead>
              <TableHead className="hidden sm:table-cell">
                <span className="sr-only">Today&apos;s trend</span>
              </TableHead>
              <TableHead className="pr-4 text-right">
                <span className="sr-only">Trade</span>
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {rows.map((quote) => {
              const isWatched = watchlist.includes(quote.ticker);
              const symbol = displaySymbol(quote.ticker);

              return (
                <TableRow key={quote.ticker}>
                  <TableCell className="pl-4">
                    <Button
                      variant="ghost"
                      size="icon"
                      className="size-8 border-transparent"
                      onClick={() => toggle(quote.ticker)}
                      aria-pressed={isWatched}
                      aria-label={isWatched ? `Remove ${symbol} from watchlist` : `Add ${symbol} to watchlist`}
                    >
                      <StarIcon className={cn(isWatched && "fill-amber-400 text-amber-400")} />
                    </Button>
                  </TableCell>
                  <TableCell>
                    <Link href={assetHref(quote.ticker)} className="group flex items-center gap-3">
                      <AssetIcon ticker={quote.ticker} assetClass={quote.assetClass} />
                      <div className="min-w-0">
                        <p className="font-semibold group-hover:underline">{symbol}</p>
                        <p className="max-w-[12rem] truncate text-xs text-muted-foreground">{quote.name ?? quote.ticker}</p>
                      </div>
                    </Link>
                  </TableCell>
                  <TableCell className="hidden md:table-cell">
                    {assetClass === "crypto" ? (
                      <Badge variant="outline" className="border-crypto/40 text-crypto">
                        24/7
                      </Badge>
                    ) : (
                      <span className="text-sm text-muted-foreground">{quote.sector ?? "—"}</span>
                    )}
                  </TableCell>
                  <TableCell className="text-right font-semibold">
                    <QuotePrice quote={quote} />
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex flex-col items-end">
                      <PercentChange ratio={quote.changePercent} className="num" />
                      <PriceChange change={quote.change} showIcon={false} className="num text-xs opacity-80" />
                    </div>
                  </TableCell>
                  <TableCell className="num hidden text-right text-xs text-muted-foreground lg:table-cell">
                    {formatCurrency(quote.dayLow)} – {formatCurrency(quote.dayHigh)}
                  </TableCell>
                  <TableCell className="hidden sm:table-cell">
                    <QuoteSparkline quote={quote} />
                  </TableCell>
                  <TableCell className="pr-4 text-right">
                    <Button size="sm" variant="outline" className="h-8" onClick={() => openTrade(quote.ticker)}>
                      Trade
                    </Button>
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </Card>
    </div>
  );
};

const SymbolSearch = () => {
  const [{ q }, setParams] = useQueryStates(marketsSearchParams);
  const [value, setValue] = useState(q);
  const searchTerm = useDebounce(value.trim(), 300);

  const updateValue = (next: string) => {
    setValue(next);
    void setParams({ q: next.trim() });
  };

  return (
    <>
      <div className="relative max-w-xl">
        <SearchIcon className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" aria-hidden="true" />
        <Input
          value={value}
          onChange={(event) => updateValue(event.target.value)}
          placeholder="Search e.g. AAPL, Tesla, ETH-USD"
          className="h-11 pl-9 pr-10"
          aria-label="Search symbols"
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
        <SearchResults searchTerm={searchTerm} />
      </QueryBoundary>
    </>
  );
};

const ResultsSkeleton = () => (
  <Card className="gap-3 p-4">
    {Array.from({ length: 8 }, (_, index) => (
      <Skeleton key={index} className="h-10 w-full" />
    ))}
  </Card>
);

const TICKER_PATTERN = /^[A-Za-z][A-Za-z0-9.\-]{0,11}$/;

const SearchResults = ({ searchTerm }: { searchTerm: string }) => {
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
        title={searchTerm ? `No companies match "${searchTerm}"` : "No symbols available yet"}
        description={
          searchTerm
            ? "Try the exact ticker symbol, or a different spelling of the company name."
            : "Search for a company to add it to the market list."
        }
        action={
          TICKER_PATTERN.test(searchTerm) ? (
            <Button asChild variant="outline">
              <Link href={assetHref(upper)}>
                Try opening {upper} directly <ArrowRightIcon />
              </Link>
            </Button>
          ) : undefined
        }
      />
    );
  }

  return (
    <Card className={cn("gap-0 overflow-hidden py-0 transition-opacity", isFetching && !isFetchingNextPage && "opacity-60")}>
      <Table>
        <TableHeader>
          <TableRow className="bg-muted/50 hover:bg-muted/50">
            <TableHead className="w-10 pl-4">
              <span className="sr-only">Watch</span>
            </TableHead>
            <TableHead>Asset</TableHead>
            <TableHead className="hidden md:table-cell">Type</TableHead>
            <TableHead className="hidden lg:table-cell">Region</TableHead>
            <TableHead className="hidden sm:table-cell">Currency</TableHead>
            <TableHead className="pr-4 text-right">
              <span className="sr-only">Trade</span>
            </TableHead>
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
                  <Link href={assetHref(ticker)} className="group flex items-center gap-3">
                    <AssetIcon ticker={ticker} />
                    <div className="min-w-0">
                      <p className="font-semibold group-hover:underline">{displaySymbol(ticker)}</p>
                      <p className="max-w-[16rem] truncate text-xs text-muted-foreground">{stock.name}</p>
                    </div>
                  </Link>
                </TableCell>
                <TableCell className="hidden text-muted-foreground md:table-cell">{stock.type}</TableCell>
                <TableCell className="hidden text-muted-foreground lg:table-cell">{stock.region}</TableCell>
                <TableCell className="hidden text-muted-foreground sm:table-cell">{stock.currency}</TableCell>
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
      <InfiniteScroll hasNextPage={hasNextPage} fetchNextPage={fetchNextPage} isFetchingNextPage={isFetchingNextPage} />
    </Card>
  );
};
