import type { Metadata } from "next";
import type { SearchParams } from "nuqs/server";
import { createLoader, parseAsString } from "nuqs/server";

import { MarketsView } from "@/modules/stocks/ui/views/markets-view";
import { MARKETS_PAGE_SIZE } from "@/modules/stocks/constants";

import { getQueryClient, HydrateClient, trpc } from "@/trpc/server";

export const metadata: Metadata = { title: "Markets" };

const loadSearchParams = createLoader({
  q: parseAsString.withDefault(""),
  tab: parseAsString.withDefault("stocks"),
});

interface PageProps {
  searchParams: Promise<SearchParams>;
}

const Page = async ({ searchParams }: PageProps) => {
  const { q, tab } = await loadSearchParams(searchParams);

  const queryClient = getQueryClient();
  await queryClient.prefetchQuery(trpc.stocks.getMarket.queryOptions());

  if (tab === "search") {
    await queryClient.prefetchInfiniteQuery(
      trpc.stocks.search.infiniteQueryOptions(
        { searchTerm: q.trim() || undefined, pageSize: MARKETS_PAGE_SIZE },
        { getNextPageParam: (lastPage) => (lastPage.hasNextPage ? lastPage.page + 1 : undefined) }
      )
    );
  }

  return (
    <HydrateClient>
      <MarketsView />
    </HydrateClient>
  );
};

export default Page;
