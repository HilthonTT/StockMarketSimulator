import type { Metadata } from "next";
import type { SearchParams } from "nuqs/server";

import { TransactionsView } from "@/modules/transactions/ui/views/transactions-view";
import { loadTransactionFilters, toTransactionQueryInput } from "@/modules/transactions/search-params";

import { getQueryClient, HydrateClient, trpc } from "@/trpc/server";

export const metadata: Metadata = { title: "History" };

interface PageProps {
  searchParams: Promise<SearchParams>;
}

const Page = async ({ searchParams }: PageProps) => {
  const filters = await loadTransactionFilters(searchParams);

  const queryClient = getQueryClient();
  void queryClient.prefetchInfiniteQuery(
    trpc.transactions.getMany.infiniteQueryOptions(toTransactionQueryInput(filters), {
      getNextPageParam: (lastPage) => lastPage.cursor ?? undefined,
    })
  );

  return (
    <HydrateClient>
      <TransactionsView />
    </HydrateClient>
  );
};

export default Page;
