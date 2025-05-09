"use client";

import { Suspense } from "react";
import { ErrorBoundary } from "react-error-boundary";
import { useSuspenseQuery } from "@tanstack/react-query";

import { useTransactionFilters } from "@/modules/transactions/hooks/use-transaction-filters";

import {
  Table,
  TableBody,
  TableCaption,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useTRPC } from "@/trpc/client";
import { PAGE_SIZE } from "@/constants";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";

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

  const [filters, setFilters] = useTransactionFilters();

  const { data: pagedTransactions } = useSuspenseQuery(
    trpc.transactions.getMany.queryOptions({
      pageSize: PAGE_SIZE,
      ...filters,
    })
  );

  const { connection, isLoading } = useSignalR();

  const handlePagination = (direction: "prev" | "next") => {
    const newPage =
      direction === "prev"
        ? filters.page - 1
        : direction === "next"
        ? filters.page + 1
        : filters.page;
    if (
      (direction === "prev" && !pagedTransactions.hasPreviousPage) ||
      (direction === "next" && !pagedTransactions.hasNextPage)
    ) {
      return;
    }

    setFilters({
      ...filters,
      page: newPage,
    });
  };

  return (
    <>
      <SearchTransactions />
      <Table
        className="rounded-lg dark:bg-black bg-white"
        aria-labelledby="transaction-table"
      >
        <TableCaption id="transaction-table">
          All of your previous transactions.
        </TableCaption>
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
          {!isLoading &&
            pagedTransactions.items.map((transaction) => (
              <TransactionWidget
                key={transaction.id}
                transaction={transaction}
                connection={connection}
              />
            ))}
        </TableBody>
      </Table>

      {/* Pagination */}
      <div className="flex items-center justify-between">
        <Button
          onClick={() => handlePagination("prev")}
          variant="elevated"
          disabled={!pagedTransactions.hasPreviousPage}
          aria-label="Previous page"
        >
          Previous
        </Button>
        <Button
          onClick={() => handlePagination("next")}
          variant="elevated"
          disabled={!pagedTransactions.hasNextPage}
          aria-label="Next page"
        >
          Next
        </Button>
      </div>
    </>
  );
};
