"use client";

import { Suspense } from "react";
import { FaPiggyBank } from "react-icons/fa";
import { ErrorBoundary } from "react-error-boundary";
import { FaArrowTrendDown, FaMedal } from "react-icons/fa6";
import { useSuspenseQuery } from "@tanstack/react-query";

import { Skeleton } from "@/components/ui/skeleton";
import { PAGE_SIZE } from "@/constants";
import { useTRPC } from "@/trpc/client";

import { DataCard } from "../components/data-card";

interface DataSectionProps {
  page: number;
}

export const DataSection = (props: DataSectionProps) => {
  return (
    <Suspense fallback={<DataSectionLoading />}>
      <ErrorBoundary fallback={<p>Error...</p>}>
        <DataSectionSuspense {...props} />
      </ErrorBoundary>
    </Suspense>
  );
};

const DataSectionLoading = () => {
  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 pb-2 mb-8">
      <Skeleton className="h-[180px] bg-white drop-shadow" />
      <Skeleton className="h-[180px] bg-white drop-shadow" />
      <Skeleton className="h-[180px] bg-white drop-shadow" />
    </div>
  );
};

const DataSectionSuspense = ({ page }: DataSectionProps) => {
  const trpc = useTRPC();

  const { data: budget } = useSuspenseQuery(trpc.budgets.getOne.queryOptions());
  const { data: pagedTransactions } = useSuspenseQuery(
    trpc.transactions.getMany.queryOptions({
      page,
      pageSize: PAGE_SIZE,
    })
  );

  const { data: topPerfomerStock } = useSuspenseQuery(
    trpc.stocks.getTopPerfomer.queryOptions()
  );

  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 pb-2 mb-8">
      <div role="region" aria-labelledby="budget-card">
        <DataCard
          title="Budget"
          value={budget.buyingPower}
          description="Available funds for investing"
          icon={FaPiggyBank}
          variant="default"
          dollarPrefix
        />
      </div>

      <div role="region" aria-labelledby="transactions-card">
        <DataCard
          title="Transactions"
          value={pagedTransactions.totalCount}
          description="Total number of buy/sell trades"
          icon={FaArrowTrendDown}
          variant="default"
        />
      </div>

      <div role="region" aria-labelledby="top-performer-card">
        <DataCard
          title="Top Performer"
          value={topPerfomerStock.ticker.toUpperCase()}
          description="Your best performing stock"
          icon={FaMedal}
          variant="default"
        />
      </div>
    </div>
  );
};
