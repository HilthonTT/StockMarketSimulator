"use client";

import { Suspense } from "react";
import { FaPiggyBank } from "react-icons/fa";
import { ErrorBoundary } from "react-error-boundary";
import { FaArrowTrendDown, FaMedal } from "react-icons/fa6";

import { trpc } from "@/trpc/client";
import { PAGE_SIZE } from "@/constants";

import { DataCard } from "../components/data-card";

interface DataSectionProps {
  page: number;
}

export const DataSection = (props: DataSectionProps) => {
  return (
    <Suspense fallback={<p>Loading...</p>}>
      <ErrorBoundary fallback={<p>Error...</p>}>
        <DataSectionSuspense {...props} />
      </ErrorBoundary>
    </Suspense>
  );
};

const DataSectionSuspense = ({ page }: DataSectionProps) => {
  const [budget] = trpc.budgets.getOne.useSuspenseQuery();
  const [pagedTransactions] = trpc.transactions.getMany.useSuspenseQuery({
    page,
    pageSize: PAGE_SIZE,
  });

  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 pb-2 mb-8">
      <DataCard
        title="Budget"
        value={budget.buyingPower}
        percentageChange={12}
        icon={FaPiggyBank}
        variant="default"
        dateRange={""}
        dollarPrefix
      />
      <DataCard
        title="Transactions"
        value={pagedTransactions.totalCount}
        percentageChange={12}
        icon={FaArrowTrendDown}
        variant="default"
        dateRange={""}
      />
      <DataCard
        title="Top Perfomer"
        value="TSLA"
        percentageChange={12}
        icon={FaMedal}
        variant="default"
        dateRange={""}
      />
    </div>
  );
};
