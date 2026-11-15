"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { WalletIcon } from "lucide-react";

import { FeedStatus } from "@/modules/market/ui/components/feed-status";
import { PercentChange } from "@/modules/market/ui/components/price-change";
import { useMarket } from "@/modules/market/hooks/use-market";

import { useTRPC } from "@/trpc/client";
import { useHydrated } from "@/hooks/use-hydrated";
import { formatCurrency } from "@/lib/utils";
import { Separator } from "@/components/ui/separator";
import { SidebarTrigger } from "@/components/ui/sidebar";
import { Skeleton } from "@/components/ui/skeleton";
import { Hint } from "@/components/hint";

import { CommandSearchTrigger } from "./command-search";

const indexFormatter = new Intl.NumberFormat("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

const IndexChip = () => {
  const { market } = useMarket();
  const hydrated = useHydrated();

  if (!market || !hydrated) {
    return null;
  }

  const { index } = market;

  return (
    <Hint label="Price-weighted index of the simulated stocks, live">
      <Link
        href="/markets"
        className="hidden items-center gap-2 rounded-md border bg-card px-2.5 py-1 text-xs hover:bg-muted lg:inline-flex"
      >
        <span className="font-semibold text-muted-foreground">{index.name}</span>
        <span className="num font-semibold">{indexFormatter.format(index.value)}</span>
        <PercentChange ratio={index.changePercent} className="num" />
      </Link>
    </Hint>
  );
};

export const AppTopbar = () => {
  const trpc = useTRPC();
  const { data: budget } = useQuery(trpc.budgets.getOne.queryOptions());

  return (
    <header className="sticky top-0 z-30 flex h-14 shrink-0 items-center gap-2 border-b bg-background/90 px-4 backdrop-blur supports-[backdrop-filter]:bg-background/75">
      <SidebarTrigger className="-ml-1" aria-label="Toggle navigation" />
      <Separator orientation="vertical" className="mr-1 data-[orientation=vertical]:h-5" />

      <CommandSearchTrigger />

      <div className="ml-auto flex items-center gap-2">
        <IndexChip />
        <FeedStatus className="hidden sm:inline-flex" />
        <Hint label="Cash available to invest">
          <Link
            href="/portfolio"
            className="inline-flex items-center gap-2 rounded-md border bg-card px-3 py-1 text-sm hover:bg-muted"
          >
            <WalletIcon className="size-4 text-muted-foreground" aria-hidden="true" />
            <span className="sr-only">Cash:</span>
            {budget ? (
              <span className="num font-semibold">{formatCurrency(budget.amount)}</span>
            ) : (
              <Skeleton className="h-4 w-16" />
            )}
          </Link>
        </Hint>
      </div>
    </header>
  );
};
