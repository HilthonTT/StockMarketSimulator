"use client";

import { Suspense, useRef, useState } from "react";
import { ErrorBoundary } from "react-error-boundary";
import { SearchIcon, XIcon } from "lucide-react";
import { useRouter, useSearchParams } from "next/navigation";
import { useSuspenseQuery } from "@tanstack/react-query";

import {
  Table,
  TableBody,
  TableCaption,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useTRPC } from "@/trpc/client";
import { APP_URL, PAGE_SIZE } from "@/constants";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

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
  const trpc = useTRPC();

  const inputRef = useRef<HTMLInputElement>(null);
  const router = useRouter();
  const searchParams = useSearchParams();

  const query = searchParams.get("query") ?? "";
  const [value, setValue] = useState(query);

  const { data: pagedTransactions } = useSuspenseQuery(
    trpc.transactions.getMany.queryOptions({
      page: page || 1,
      pageSize: PAGE_SIZE,
      searchTerm: query || undefined,
    })
  );

  const { connection, isLoading } = useSignalR();

  const updateURL = (newQuery?: string, newPage?: number) => {
    const url = new URL("/", APP_URL);

    if (newQuery) {
      url.searchParams.set("query", newQuery);
    }
    if (newPage) {
      url.searchParams.set("page", newPage.toString());
    }

    router.push(url.toString());
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setValue(e.target.value);
  };

  const handleClear = () => {
    if (!value) {
      return;
    }

    updateURL(undefined, page);

    setValue("");

    inputRef.current?.blur();
  };

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const trimmed = value.trim();
    updateURL(trimmed || undefined, page);
    inputRef.current?.blur();
  };

  const handlePagination = (direction: "prev" | "next") => {
    const newPage =
      direction === "prev" ? page - 1 : direction === "next" ? page + 1 : page;
    if (
      (direction === "prev" && !pagedTransactions.hasPreviousPage) ||
      (direction === "next" && !pagedTransactions.hasNextPage)
    ) {
      return;
    }

    updateURL(query || undefined, newPage);
  };

  return (
    <>
      <form onSubmit={handleSubmit} className="relative max-w-[720px] w-full">
        <Input
          ref={inputRef}
          value={value}
          onChange={handleChange}
          className="md:text-base px-14 w-full border-none focus-visible:shadow-[0_1px_1px_0_rgba(65,69,73,.3),0_1px_3px_1px_rgba(65,69,73,.15)]  rounded-full h-[48px] focus-visible:ring-0 focus:bg-white4"
          placeholder="Search for transactions..."
          aria-label="Search for transactions"
        />

        <Button
          type="submit"
          variant="ghost"
          size="icon"
          className="absolute left-3 top-1/2 -translate-y-1/2 [&_svg]:size-5 rounded-full"
          aria-label="Search"
        >
          <SearchIcon />
        </Button>

        <Button
          onClick={handleClear}
          type="button"
          variant="ghost"
          size="icon"
          className={cn(
            "absolute right-3 top-1/2 -translate-y-1/2 [&_svg]:size-5 rounded-full transition opacity-0 cursor-default",
            value && "opacity-100 cursor-pointer"
          )}
          aria-label="Clear search"
        >
          <XIcon />
        </Button>
      </form>
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
