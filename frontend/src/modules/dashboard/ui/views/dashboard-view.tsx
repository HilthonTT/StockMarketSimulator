"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { GlobeIcon, SearchIcon } from "lucide-react";

import { useCommandSearch } from "@/modules/shell/hooks/use-command-search";
import { useMarket } from "@/modules/market/hooks/use-market";
import { IndexCard, MoversCard, NewsFeed } from "@/modules/market/ui/components/market-widgets";
import { usePortfolio } from "@/modules/portfolio/hooks/use-portfolio";
import { AccountValueChart } from "@/modules/portfolio/ui/components/account-value-chart";

import { useTRPC } from "@/trpc/client";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { PageHeader } from "@/components/page-header";
import { QueryBoundary } from "@/components/query-boundary";

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
        description="Your simulated account, the market and the latest headlines at a glance."
        actions={
          <>
            <Button variant="outline" onClick={() => openSearch(true)}>
              <SearchIcon /> Find an asset
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
        <div className="flex min-w-0 flex-col gap-6 lg:col-span-2">
          <QueryBoundary fallback={<Skeleton className="h-[420px] w-full rounded-xl" />} errorTitle="Couldn't load your account">
            <AccountHero />
          </QueryBoundary>
          <MarketWatchSection />
        </div>
        <div className="flex min-w-0 flex-col gap-6">
          <MarketPulse />
          <HoldingsPreviewSection />
          <NewsFeed limit={5} />
          <RecentActivitySection />
        </div>
      </div>
    </>
  );
};

/** The account value chart, once the user owns something. */
const AccountHero = () => {
  const portfolio = usePortfolio();

  if (portfolio.holdings.length === 0) {
    return null;
  }

  return <AccountValueChart portfolio={portfolio} />;
};

/** Index level plus today's movers, in one compact card stack. */
const MarketPulse = () => {
  const { market, isLoading } = useMarket();

  if (isLoading) {
    return <Skeleton className="h-80 w-full rounded-xl" />;
  }

  if (!market) {
    return null;
  }

  return (
    <>
      <IndexCard index={market.index} />
      <Tabs defaultValue="gainers" className="gap-3">
        <TabsList className="w-full">
          <TabsTrigger value="gainers">Gainers</TabsTrigger>
          <TabsTrigger value="losers">Losers</TabsTrigger>
        </TabsList>
        <TabsContent value="gainers">
          <MoversCard title="Top gainers today" quotes={market.gainers} direction="up" />
        </TabsContent>
        <TabsContent value="losers">
          <MoversCard title="Top losers today" quotes={market.losers} direction="down" />
        </TabsContent>
      </Tabs>
    </>
  );
};
