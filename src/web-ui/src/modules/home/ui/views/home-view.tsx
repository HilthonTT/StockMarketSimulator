"use client";

import { Suspense } from "react";
import { ErrorBoundary } from "react-error-boundary";

import { ChartSection } from "../sections/chart-section";
import { DataSection } from "../sections/data-section";
import { TransactionSection } from "../sections/transactions-section";
import { SearchStockSection } from "../sections/search-stock-section";

interface HomeViewProps {
  page: number;
}

export const HomeView = (props: HomeViewProps) => {
  return (
    <Suspense fallback={<p>Loading...</p>}>
      <ErrorBoundary fallback={<p>Error...</p>}>
        <HomeViewSuspense {...props} />
      </ErrorBoundary>
    </Suspense>
  );
};

const HomeViewSuspense = ({ page }: HomeViewProps) => {
  return (
    <>
      <SearchStockSection />
      <div className="max-w-[2400px] mx-auto mb-10 px-4 pt-2.5 flex flex-col gap-y-6 pb-24">
        <ChartSection />
        <DataSection page={page} />
        <TransactionSection page={page} />
      </div>
    </>
  );
};
