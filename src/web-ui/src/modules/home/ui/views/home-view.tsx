"use client";

import { Suspense } from "react";
import { ErrorBoundary } from "react-error-boundary";

import { trpc } from "@/trpc/client";

import { ChartSection } from "../sections/chart-section";
import { DataSection } from "../sections/data-section";
import { TransactionSection } from "../sections/transactions-section";

export const HomeView = () => {
  return (
    <Suspense fallback={<p>Loading...</p>}>
      <ErrorBoundary fallback={<p>Error...</p>}>
        <HomeViewSuspense />
      </ErrorBoundary>
    </Suspense>
  );
};

const HomeViewSuspense = () => {
  const [stocks] = trpc.stocks.getMany.useSuspenseQuery();

  console.log({ stocks });

  return (
    <div className="max-w-[2400px] mx-auto mb-10 px-4 pt-2.5 flex flex-col gap-y-6 pb-24">
      <ChartSection />
      <DataSection />
      <TransactionSection />
    </div>
  );
};
