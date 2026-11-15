"use client";

import Link from "next/link";
import { formatDistanceToNow } from "date-fns";
import { NewspaperIcon, TrendingDownIcon, TrendingUpIcon } from "lucide-react";

import type { MarketIndex } from "@/modules/stocks/types";

import { cn, formatCurrency, formatPercent, parseUtc } from "@/lib/utils";
import { assetHref, displaySymbol } from "@/lib/assets";
import { AssetIcon } from "@/components/asset-icon";
import { Sparkline } from "@/components/sparkline";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

import type { Quote } from "../../store";
import { useSparkline } from "../../hooks/use-price-history";
import { useFeedStore } from "../../store";
import { LivePrice } from "./live-price";
import { PercentChange, PriceChange } from "./price-change";

const indexFormatter = new Intl.NumberFormat("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/** Headline card for the simulated index. */
export const IndexCard = ({ index, breadth }: { index: MarketIndex; breadth?: { up: number; down: number } }) => {
  const total = breadth ? breadth.up + breadth.down : 0;

  return (
    <Card className="gap-3">
      <CardHeader>
        <CardDescription className="flex items-center justify-between">
          <span className="font-semibold uppercase tracking-wide">{index.name}</span>
          <span className="text-xs">Price-weighted · live</span>
        </CardDescription>
        <CardTitle className="num text-3xl">{indexFormatter.format(index.value)}</CardTitle>
        <PriceChange change={index.change} ratio={index.changePercent} className="text-sm" />
      </CardHeader>
      {breadth && total > 0 && (
        <CardContent className="flex flex-col gap-1.5">
          <div className="flex h-1.5 overflow-hidden rounded-full bg-muted" aria-hidden="true">
            <span className="bg-gain" style={{ width: `${(breadth.up / total) * 100}%` }} />
            <span className="bg-loss" style={{ width: `${(breadth.down / total) * 100}%` }} />
          </div>
          <p className="num text-xs text-muted-foreground">
            <span className="text-gain">{breadth.up} advancing</span> · <span className="text-loss">{breadth.down} declining</span>
          </p>
        </CardContent>
      )}
    </Card>
  );
};

const MoverRow = ({ quote }: { quote: Quote }) => (
  <li>
    <Link
      href={assetHref(quote.ticker)}
      className="-mx-2 flex items-center gap-3 rounded-md px-2 py-2 hover:bg-muted/60"
    >
      <AssetIcon ticker={quote.ticker} assetClass={quote.assetClass} size="sm" />
      <div className="min-w-0 flex-1">
        <p className="text-sm font-semibold">{displaySymbol(quote.ticker)}</p>
        <p className="truncate text-xs text-muted-foreground">{quote.name ?? quote.ticker}</p>
      </div>
      <div className="text-right">
        <p className="num text-sm">{formatCurrency(quote.price)}</p>
        <PercentChange ratio={quote.changePercent} className="num text-xs" />
      </div>
    </Link>
  </li>
);

/** Top gainers or losers of the day. */
export const MoversCard = ({ title, quotes, direction }: { title: string; quotes: Quote[]; direction: "up" | "down" }) => {
  const Icon = direction === "up" ? TrendingUpIcon : TrendingDownIcon;

  return (
    <Card className="gap-2">
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base">
          <Icon className={cn("size-4", direction === "up" ? "text-gain" : "text-loss")} aria-hidden="true" />
          {title}
        </CardTitle>
      </CardHeader>
      <CardContent>
        {quotes.length === 0 ? (
          <p className="py-6 text-center text-sm text-muted-foreground">
            Nothing {direction === "up" ? "up" : "down"} yet today.
          </p>
        ) : (
          <ul>
            {quotes.map((quote) => (
              <MoverRow key={quote.ticker} quote={quote} />
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  );
};

/** Live market news (seeded from the API, then pushed over SignalR). */
export const NewsFeed = ({ limit = 6, className }: { limit?: number; className?: string }) => {
  const news = useFeedStore((state) => state.news);
  const items = news.slice(0, limit);

  return (
    <Card className={cn("gap-2", className)}>
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base">
          <NewspaperIcon className="size-4 text-muted-foreground" aria-hidden="true" />
          Market news
          <span className="ml-auto size-2 animate-pulse rounded-full bg-primary" aria-hidden="true" />
        </CardTitle>
        <CardDescription>Simulated headlines that move prices.</CardDescription>
      </CardHeader>
      <CardContent>
        {items.length === 0 ? (
          <p className="py-6 text-center text-sm text-muted-foreground">No headlines yet. Stay tuned.</p>
        ) : (
          <ul className="divide-y" aria-live="polite">
            {items.map((item) => (
              <li key={item.id} className="flex gap-3 py-2.5">
                <AssetIcon ticker={item.ticker} size="sm" />
                <div className="min-w-0 flex-1">
                  <p className="text-sm leading-snug">{item.headline}</p>
                  <p className="mt-0.5 flex items-center gap-2 text-xs text-muted-foreground">
                    <Link href={assetHref(item.ticker)} className="font-semibold hover:underline">
                      {displaySymbol(item.ticker)}
                    </Link>
                    <span className={cn("num", item.impactPercent >= 0 ? "text-gain" : "text-loss")}>
                      {formatPercent(item.impactPercent)}
                    </span>
                    <span>{formatDistanceToNow(parseUtc(item.publishedOnUtc), { addSuffix: true })}</span>
                  </p>
                </div>
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  );
};

/** Sparkline of today's prices for table rows. */
export const QuoteSparkline = ({ quote, className }: { quote: Quote; className?: string }) => {
  const values = useSparkline(quote.ticker);

  return <Sparkline values={values} baseline={quote.previousClose} className={className} />;
};

/** Live price cell that flashes on ticks. */
export const QuotePrice = ({ quote }: { quote: Quote }) => <LivePrice quote={quote} className="num" />;
