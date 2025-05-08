import type { SearchParams } from "nuqs/server";
import { redirect } from "next/navigation";

import { HomeView } from "@/modules/home/ui/views/home-view";
import { loadTransactionFilters } from "@/modules/transactions/search-params";

import { caller, getQueryClient, HydrateClient, trpc } from "@/trpc/server";
import { PAGE_SIZE } from "@/constants";

interface PageProps {
  searchParams: Promise<SearchParams>;
}

const Page = async ({ searchParams }: PageProps) => {
  const user = await caller.auth.current();
  if (!user) {
    return redirect("/login");
  }

  const filters = await loadTransactionFilters(searchParams);

  const queryClient = getQueryClient();

  void queryClient.prefetchQuery(trpc.auth.getJwt.queryOptions());
  void queryClient.prefetchQuery(trpc.budgets.getOne.queryOptions());
  void queryClient.prefetchQuery(trpc.stocks.getTopPerfomer.queryOptions());
  void queryClient.prefetchQuery(
    trpc.transactions.getMany.queryOptions({
      pageSize: PAGE_SIZE,
      ...filters,
    })
  );
  void queryClient.prefetchQuery(
    trpc.users.getPurchasedStockTickers.queryOptions()
  );

  return (
    <HydrateClient>
      <HomeView />
    </HydrateClient>
  );
};

export default Page;
