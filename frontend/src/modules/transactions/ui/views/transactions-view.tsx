"use client";

import Link from "next/link";
import { format } from "date-fns";
import { useSuspenseInfiniteQuery } from "@tanstack/react-query";
import { ReceiptTextIcon } from "lucide-react";

import { useTRPC } from "@/trpc/client";
import { cn, formatCurrency, formatNumber, parseUtc } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { PageHeader } from "@/components/page-header";
import { QueryBoundary } from "@/components/query-boundary";
import { EmptyState } from "@/components/empty-state";
import { InfiniteScroll } from "@/components/infinite-scroll";

import { useTransactionFilters } from "../../hooks/use-transaction-filters";
import { toTransactionQueryInput } from "../../search-params";
import { TransactionFilters } from "../components/transaction-filters";
import { isBuy, SideBadge } from "../components/side-badge";

export const TransactionsView = () => {
  return (
    <>
      <PageHeader title="History" description="Every order you've placed, newest first." />
      <TransactionFilters />
      <QueryBoundary
        fallback={<Skeleton className="h-[480px] w-full rounded-xl" />}
        errorTitle="Couldn't load your history"
      >
        <TransactionsTable />
      </QueryBoundary>
    </>
  );
};

const TransactionsTable = () => {
  const trpc = useTRPC();
  const [filters, setFilters] = useTransactionFilters();

  const { data, hasNextPage, fetchNextPage, isFetchingNextPage } = useSuspenseInfiniteQuery(
    trpc.transactions.getMany.infiniteQueryOptions(toTransactionQueryInput(filters), {
      getNextPageParam: (lastPage) => lastPage.cursor ?? undefined,
    })
  );

  const transactions = data.pages.flatMap((page) => page.data);
  const isFiltered = !!filters.search || !!filters.from || !!filters.to;

  if (transactions.length === 0) {
    return (
      <EmptyState
        icon={ReceiptTextIcon}
        title={isFiltered ? "No trades match these filters" : "No trades yet"}
        description={
          isFiltered ? "Try a different ticker or date range." : "Buy your first stock and it will show up here."
        }
        action={
          isFiltered ? (
            <Button variant="outline" onClick={() => setFilters({ search: "", from: null, to: null })}>
              Clear filters
            </Button>
          ) : (
            <Button asChild>
              <Link href="/markets">Browse markets</Link>
            </Button>
          )
        }
      />
    );
  }

  return (
    <Card className="gap-0 overflow-hidden py-0">
      <Table>
        <TableHeader>
          <TableRow className="bg-muted/50 hover:bg-muted/50">
            <TableHead className="pl-4">Date</TableHead>
            <TableHead>Side</TableHead>
            <TableHead>Symbol</TableHead>
            <TableHead className="text-right">Shares</TableHead>
            <TableHead className="text-right">Price</TableHead>
            <TableHead className="pr-4 text-right">Total</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {transactions.map((transaction) => {
            const createdOn = parseUtc(transaction.createdOnUtc);
            const ticker = transaction.ticker.toUpperCase();
            const buy = isBuy(transaction.type);

            return (
              <TableRow key={transaction.id}>
                <TableCell className="pl-4">
                  <div className="flex flex-col">
                    <span>{format(createdOn, "MMM d, yyyy")}</span>
                    <span className="text-xs text-muted-foreground">{format(createdOn, "HH:mm:ss")}</span>
                  </div>
                </TableCell>
                <TableCell>
                  <SideBadge type={transaction.type} />
                </TableCell>
                <TableCell>
                  <Link href={`/stocks/${ticker}`} className="font-semibold hover:underline">
                    {ticker}
                  </Link>
                </TableCell>
                <TableCell className="text-right tabular-nums">{formatNumber(transaction.quantity)}</TableCell>
                <TableCell className="text-right tabular-nums">{formatCurrency(transaction.amount)}</TableCell>
                <TableCell className={cn("pr-4 text-right font-medium tabular-nums", !buy && "text-gain")}>
                  {buy ? "-" : "+"}
                  {formatCurrency(transaction.amount * transaction.quantity)}
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
      <InfiniteScroll
        hasNextPage={hasNextPage}
        fetchNextPage={fetchNextPage}
        isFetchingNextPage={isFetchingNextPage}
      />
    </Card>
  );
};
