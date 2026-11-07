"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { ArrowRightIcon, LoaderIcon, SearchIcon, StarIcon, TrendingUpIcon } from "lucide-react";

import { useWatchlist } from "@/modules/market/hooks/use-watchlist";

import { useTRPC } from "@/trpc/client";
import { useDebounce } from "@/hooks/use-debounce";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
  CommandSeparator,
} from "@/components/ui/command";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog";

import { accountNav, tradingNav } from "../../nav";
import { useCommandSearch } from "../../hooks/use-command-search";

const TICKER_PATTERN = /^[A-Za-z][A-Za-z0-9.\-]{0,9}$/;

/** Search button for the top bar; also opens with Ctrl/⌘+K or "/". */
export const CommandSearchTrigger = ({ className }: { className?: string }) => {
  const setOpen = useCommandSearch((state) => state.setOpen);

  return (
    <Button
      variant="outline"
      onClick={() => setOpen(true)}
      className={cn(
        "h-9 justify-start gap-2 text-sm font-normal text-muted-foreground shadow-none sm:w-64 lg:w-80",
        className
      )}
      aria-label="Search stocks"
    >
      <SearchIcon className="size-4" />
      <span className="hidden sm:inline">Search stocks…</span>
      <kbd className="pointer-events-none ml-auto hidden select-none items-center gap-0.5 rounded border bg-muted px-1.5 font-mono text-[10px] font-medium sm:inline-flex">
        Ctrl K
      </kbd>
    </Button>
  );
};

export const CommandSearch = () => {
  const router = useRouter();
  const trpc = useTRPC();
  const { isOpen, setOpen } = useCommandSearch();
  const watchlist = useWatchlist((state) => state.tickers);

  const [search, setSearch] = useState("");
  const debouncedSearch = useDebounce(search.trim(), 250);

  useEffect(() => {
    const onKeyDown = (event: KeyboardEvent) => {
      const target = event.target as HTMLElement | null;
      const isTyping =
        target?.isContentEditable || ["INPUT", "TEXTAREA", "SELECT"].includes(target?.tagName ?? "");

      if ((event.key === "k" && (event.metaKey || event.ctrlKey)) || (event.key === "/" && !isTyping)) {
        event.preventDefault();
        setOpen(!useCommandSearch.getState().isOpen);
      }
    };

    document.addEventListener("keydown", onKeyDown);
    return () => document.removeEventListener("keydown", onKeyDown);
  }, [setOpen]);

  const { data: results, isFetching } = useQuery(
    trpc.stocks.search.queryOptions(
      { searchTerm: debouncedSearch, pageSize: 8 },
      { enabled: isOpen && debouncedSearch.length > 0, placeholderData: (previous) => previous }
    )
  );

  const { data: positions } = useQuery({
    ...trpc.portfolio.getPositions.queryOptions(),
    enabled: isOpen,
  });

  const handleOpenChange = (open: boolean) => {
    setOpen(open);
    if (!open) {
      setSearch("");
    }
  };

  const go = (href: string) => {
    handleOpenChange(false);
    router.push(href);
  };

  const query = search.trim();
  const upperQuery = query.toUpperCase();
  const stockResults = debouncedSearch ? (results?.items ?? []) : [];
  const showDirectTicker =
    TICKER_PATTERN.test(query) && !stockResults.some((stock) => stock.ticker.toUpperCase() === upperQuery);

  const pages = [...tradingNav, ...accountNav].filter(
    (item) => !query || item.title.toLowerCase().includes(query.toLowerCase())
  );

  const quickTickers = [
    ...new Set([...(positions ?? []).map((position) => position.ticker), ...watchlist]),
  ].slice(0, 8);

  return (
    <Dialog open={isOpen} onOpenChange={handleOpenChange}>
      <DialogContent className="overflow-hidden p-0 sm:max-w-lg [&>button:last-child]:hidden">
        <DialogHeader className="sr-only">
          <DialogTitle>Search</DialogTitle>
          <DialogDescription>Search for a stock by ticker or company name, or jump to a page.</DialogDescription>
        </DialogHeader>
        <Command
          shouldFilter={false}
          className="[&_[cmdk-group-heading]]:px-2 [&_[cmdk-group-heading]]:font-medium [&_[cmdk-group-heading]]:text-muted-foreground [&_[cmdk-item]]:px-2 [&_[cmdk-item]]:py-2.5"
        >
          <CommandInput
            value={search}
            onValueChange={setSearch}
            placeholder="Ticker or company, e.g. AAPL or Microsoft"
          />
          <CommandList className="max-h-[60vh]">
            <CommandEmpty>
              {isFetching ? (
                <span className="inline-flex items-center gap-2">
                  <LoaderIcon className="size-4 animate-spin" /> Searching…
                </span>
              ) : (
                "No matches. Try a ticker symbol like MSFT."
              )}
            </CommandEmpty>

            {showDirectTicker && (
              <CommandGroup heading="Ticker">
                <CommandItem value={`direct-${upperQuery}`} onSelect={() => go(`/stocks/${upperQuery}`)}>
                  <ArrowRightIcon />
                  Open <span className="font-semibold">{upperQuery}</span>
                </CommandItem>
              </CommandGroup>
            )}

            {stockResults.length > 0 && (
              <CommandGroup heading="Stocks">
                {stockResults.map((stock) => (
                  <CommandItem
                    key={stock.ticker}
                    value={`stock-${stock.ticker}`}
                    onSelect={() => go(`/stocks/${stock.ticker.toUpperCase()}`)}
                  >
                    <TrendingUpIcon />
                    <span className="w-16 shrink-0 font-semibold">{stock.ticker.toUpperCase()}</span>
                    <span className="truncate text-muted-foreground">{stock.name}</span>
                    <span className="ml-auto shrink-0 text-xs text-muted-foreground">{stock.region}</span>
                  </CommandItem>
                ))}
              </CommandGroup>
            )}

            {!query && quickTickers.length > 0 && (
              <CommandGroup heading="Holdings & watchlist">
                {quickTickers.map((ticker) => (
                  <CommandItem key={ticker} value={`quick-${ticker}`} onSelect={() => go(`/stocks/${ticker}`)}>
                    <StarIcon />
                    <span className="font-semibold">{ticker}</span>
                  </CommandItem>
                ))}
              </CommandGroup>
            )}

            {pages.length > 0 && (
              <>
                <CommandSeparator />
                <CommandGroup heading="Go to">
                  {pages.map((item) => (
                    <CommandItem key={item.href} value={`page-${item.href}`} onSelect={() => go(item.href)}>
                      <item.icon />
                      {item.title}
                      <span className="ml-auto text-xs text-muted-foreground">{item.description}</span>
                    </CommandItem>
                  ))}
                </CommandGroup>
              </>
            )}
          </CommandList>
        </Command>
      </DialogContent>
    </Dialog>
  );
};
