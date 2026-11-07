"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { WalletIcon } from "lucide-react";

import { FeedStatus } from "@/modules/market/ui/components/feed-status";

import { useTRPC } from "@/trpc/client";
import { formatCurrency } from "@/lib/utils";
import { Separator } from "@/components/ui/separator";
import { SidebarTrigger } from "@/components/ui/sidebar";
import { Skeleton } from "@/components/ui/skeleton";
import { Hint } from "@/components/hint";

import { CommandSearchTrigger } from "./command-search";

export const AppTopbar = () => {
  const trpc = useTRPC();
  const { data: budget } = useQuery(trpc.budgets.getOne.queryOptions());

  return (
    <header className="sticky top-0 z-30 flex h-14 shrink-0 items-center gap-2 border-b bg-background/95 px-4 backdrop-blur supports-[backdrop-filter]:bg-background/80">
      <SidebarTrigger className="-ml-1" aria-label="Toggle navigation" />
      <Separator orientation="vertical" className="mr-1 data-[orientation=vertical]:h-5" />

      <CommandSearchTrigger />

      <div className="ml-auto flex items-center gap-2">
        <FeedStatus className="hidden sm:inline-flex" />
        <Hint label="Cash available to invest">
          <Link
            href="/portfolio"
            className="inline-flex items-center gap-2 rounded-full border px-3 py-1 text-sm hover:bg-muted"
          >
            <WalletIcon className="size-4 text-muted-foreground" aria-hidden="true" />
            <span className="sr-only">Cash:</span>
            {budget ? (
              <span className="font-semibold tabular-nums">{formatCurrency(budget.amount)}</span>
            ) : (
              <Skeleton className="h-4 w-16" />
            )}
          </Link>
        </Hint>
      </div>
    </header>
  );
};
