"use client";

import { Suspense } from "react";
import { ErrorBoundary } from "react-error-boundary";
import { SearchIcon } from "lucide-react";

import {
  Table,
  TableBody,
  TableCaption,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { trpc } from "@/trpc/client";
import { PAGE_SIZE } from "@/constants";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

import { TransactionWidget } from "../components/transaction-widget";
import { useSignalR } from "../../hooks/use-signalr";

interface TransactionSectionProps {
  page: number;
}

export const TransactionSection = (props: TransactionSectionProps) => {
  return (
    <Suspense fallback={<p>Loading...</p>}>
      <ErrorBoundary fallback={<p>Error</p>}>
        <TransactionSectionSuspense {...props} />
      </ErrorBoundary>
    </Suspense>
  );
};

const TransactionSectionSuspense = ({ page }: TransactionSectionProps) => {
  const [pagedTransactions] = trpc.transactions.getMany.useSuspenseQuery({
    page: page || 1,
    pageSize: PAGE_SIZE,
  });

  const { connection, isLoading } = useSignalR();

  return (
    <>
      <div className="flex w-full items-center relative">
        <Input
          className="mr-auto lg:w-1/3 w-full pl-8"
          placeholder="Search for transactions..."
        />
        <SearchIcon className="absolute top-1/2 -bottom-1/2 -translate-y-1/2 size-4 translate-x-1/2" />
      </div>
      <Table className="rounded-lg dark:bg-black bg-white">
        <TableCaption>All of your previous transactions.</TableCaption>
        <TableHeader className="h-14">
          <TableRow>
            <TableHead className="w-[200px] text-bas lg:text-2xl font-bold">
              SYMBOL
            </TableHead>
            <TableHead className="lg:text-2xl text-base">
              Bought Price Per Unit
            </TableHead>
            <TableHead className="lg:text-2xl text-base">Units</TableHead>
            <TableHead className="text-right lg:text-2xl text-base">
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

      <div className="flex items-center justify-between">
        <Button
          variant="elevated"
          disabled={!pagedTransactions.hasPreviousPage}
        >
          Previous
        </Button>
        <Button variant="elevated" disabled={!pagedTransactions.hasNextPage}>
          Next
        </Button>
      </div>
    </>
  );
};
