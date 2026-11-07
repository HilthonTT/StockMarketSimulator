"use client";

import Link from "next/link";
import { useSuspenseQuery } from "@tanstack/react-query";
import { ArrowRightIcon, BriefcaseBusinessIcon, ReceiptTextIcon } from "lucide-react";

import { usePortfolio } from "@/modules/portfolio/hooks/use-portfolio";
import { LivePrice } from "@/modules/market/ui/components/live-price";
import { PercentChange } from "@/modules/market/ui/components/price-change";
import { TransactionList } from "@/modules/transactions/ui/components/transaction-list";

import { useTRPC } from "@/trpc/client";
import { formatNumber } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Card, CardAction, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { QueryBoundary } from "@/components/query-boundary";
import { EmptyState } from "@/components/empty-state";

import { RECENT_ACTIVITY_SIZE } from "../../constants";

const SectionCard = ({
  title,
  href,
  linkLabel,
  children,
}: {
  title: string;
  href: string;
  linkLabel: string;
  children: React.ReactNode;
}) => (
  <Card className="gap-2">
    <CardHeader>
      <CardTitle>{title}</CardTitle>
      <CardAction>
        <Button asChild variant="ghost" size="sm" className="h-8 border-transparent">
          <Link href={href}>
            {linkLabel}
            <ArrowRightIcon />
          </Link>
        </Button>
      </CardAction>
    </CardHeader>
    <CardContent>{children}</CardContent>
  </Card>
);

const ListSkeleton = () => (
  <div className="flex flex-col gap-3 py-2">
    {Array.from({ length: 4 }, (_, index) => (
      <Skeleton key={index} className="h-10 w-full" />
    ))}
  </div>
);

export const HoldingsPreviewSection = () => (
  <SectionCard title="Top holdings" href="/portfolio" linkLabel="Portfolio">
    <QueryBoundary fallback={<ListSkeleton />} errorTitle="Couldn't load holdings">
      <HoldingsPreview />
    </QueryBoundary>
  </SectionCard>
);

const HoldingsPreview = () => {
  const { holdings } = usePortfolio();

  if (holdings.length === 0) {
    return (
      <EmptyState
        icon={BriefcaseBusinessIcon}
        title="No holdings yet"
        description="Stocks you buy show up here with their live value."
        className="py-8"
      />
    );
  }

  return (
    <ul className="divide-y">
      {holdings.slice(0, 5).map((holding) => (
        <li key={holding.ticker}>
          <Link
            href={`/stocks/${holding.ticker}`}
            className="-mx-2 flex items-center justify-between gap-3 rounded-md px-2 py-3 hover:bg-muted/60"
          >
            <div>
              <p className="font-semibold">{holding.ticker}</p>
              <p className="text-xs text-muted-foreground">{formatNumber(holding.quantity)} shares</p>
            </div>
            <div className="text-right text-sm">
              <p className="font-medium">
                <LivePrice quote={holding.quote} />
              </p>
              <PercentChange ratio={holding.unrealizedPnlRatio} className="text-xs" />
            </div>
          </Link>
        </li>
      ))}
    </ul>
  );
};

export const RecentActivitySection = () => (
  <SectionCard title="Recent activity" href="/transactions" linkLabel="History">
    <QueryBoundary fallback={<ListSkeleton />} errorTitle="Couldn't load your activity">
      <RecentActivity />
    </QueryBoundary>
  </SectionCard>
);

const RecentActivity = () => {
  const trpc = useTRPC();
  const { data } = useSuspenseQuery(
    trpc.transactions.getMany.queryOptions({ pageSize: RECENT_ACTIVITY_SIZE })
  );

  if (data.data.length === 0) {
    return (
      <EmptyState
        icon={ReceiptTextIcon}
        title="No trades yet"
        description="Your buys and sells will appear here."
        className="py-8"
      />
    );
  }

  return <TransactionList transactions={data.data} />;
};
