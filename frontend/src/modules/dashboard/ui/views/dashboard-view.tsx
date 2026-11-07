"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { GlobeIcon, SearchIcon } from "lucide-react";

import { useCommandSearch } from "@/modules/shell/hooks/use-command-search";

import { useTRPC } from "@/trpc/client";
import { Button } from "@/components/ui/button";
import { PageHeader } from "@/components/page-header";

import { AccountSummarySection } from "../sections/account-summary-section";
import { MarketWatchSection } from "../sections/market-watch-section";
import { HoldingsPreviewSection, RecentActivitySection } from "../sections/side-sections";

const greeting = () => {
  const hour = new Date().getHours();
  if (hour < 12) return "Good morning";
  if (hour < 18) return "Good afternoon";
  return "Good evening";
};

export const DashboardView = () => {
  const trpc = useTRPC();
  const { data: user } = useQuery(trpc.users.getCurrent.queryOptions());
  const openSearch = useCommandSearch((state) => state.setOpen);

  return (
    <>
      <PageHeader
        title={
          <span suppressHydrationWarning>
            {greeting()}
            {user ? `, ${user.username}` : ""}
          </span>
        }
        description="Here's how your simulated portfolio is doing right now."
        actions={
          <>
            <Button variant="outline" onClick={() => openSearch(true)}>
              <SearchIcon /> Find a stock
            </Button>
            <Button asChild>
              <Link href="/markets">
                <GlobeIcon /> Markets
              </Link>
            </Button>
          </>
        }
      />

      <AccountSummarySection />

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="min-w-0 lg:col-span-2">
          <MarketWatchSection />
        </div>
        <div className="flex min-w-0 flex-col gap-6">
          <HoldingsPreviewSection />
          <RecentActivitySection />
        </div>
      </div>
    </>
  );
};
