"use client";

import { Suspense } from "react";
import { ErrorBoundary } from "react-error-boundary";
import { useSuspenseInfiniteQuery } from "@tanstack/react-query";

import { useTransactionFilters } from "@/modules/transactions/hooks/use-transaction-filters";

import {
  Table,
  TableBody,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useTRPC } from "@/trpc/client";
import { PAGE_SIZE } from "@/constants";
import { Skeleton } from "@/components/ui/skeleton";
import { InfiniteScroll } from "@/components/infinite-scroll";

import { TransactionWidget } from "../components/transaction-widget";
import { useSignalR } from "../../hooks/use-signalr";
import { SearchTransactions } from "../components/search-transactions";

export const TransactionSection = () => {
  return (
    <Suspense fallback={<TransactionSectionLoading />}>
      <ErrorBoundary fallback={<p>Error</p>}>
        <TransactionSectionSuspense />
      </ErrorBoundary>
    </Suspense>
  );
};

const TransactionSectionLoading = () => {
  return (
    <>
      <Skeleton className="w-[720px] bg-white dark:bg-black h-[50px] rounded-full" />
      <Skeleton className="w-full h-[500px] bg-white dark:bg-black" />
    </>
  );
};

const TransactionSectionSuspense = () => {
  const trpc = useTRPC();

  const [filters] = useTransactionFilters();

  const {
    data,
    isLoading: dataLoading,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useSuspenseInfiniteQuery(
    trpc.transactions.getMany.infiniteQueryOptions(
      {
        pageSize: PAGE_SIZE,
        ...filters,
      },
      {
        getNextPageParam: (lastPage) => lastPage.cursor ?? undefined,
      }
    )
  );

  const { connection, isLoading } = useSignalR();

  return (
    <>
      <SearchTransactions />
      <Table
        className="rounded-lg dark:bg-black bg-white"
        aria-labelledby="transaction-table"
      >
        <TableHeader className="h-14">
          <TableRow>
            <TableHead
              className="w-[200px] text-bas lg:text-2xl font-bold"
              aria-sort="none"
            >
              SYMBOL
            </TableHead>
            <TableHead className="lg:text-2xl text-base" aria-sort="none">
              Original Price
            </TableHead>
            <TableHead className="lg:text-2xl text-base" aria-sort="none">
              Bought Price Per Unit
            </TableHead>
            <TableHead className="lg:text-2xl text-base" aria-sort="none">
              Units
            </TableHead>
            <TableHead
              className="text-right lg:text-2xl text-base"
              aria-sort="none"
            >
              Total Price
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {!dataLoading && !isLoading && data && (
            <>
              {data.pages
                .flatMap((page) => page.data)
                .map((transaction) => (
                  <TransactionWidget
                    key={transaction.id}
                    transaction={transaction}
                    connection={connection}
                  />
                ))}
            </>
          )}
        </TableBody>
      </Table>

      <InfiniteScroll
        hasNextPage={hasNextPage}
        fetchNextPage={fetchNextPage}
        isFetchingNextPage={isFetchingNextPage}
      />
    </>
  );
};
