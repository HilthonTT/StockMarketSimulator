import { redirect } from "next/navigation";

import { HomeView } from "@/modules/home/ui/views/home-view";

import { caller, getQueryClient, HydrateClient, trpc } from "@/trpc/server";
import { PAGE_SIZE } from "@/constants";

interface PageProps {
  searchParams: Promise<{
    page?: string;
    search?: string;
  }>;
}

const Page = async ({ searchParams }: PageProps) => {
  const user = await caller.auth.current();
  if (!user) {
    return redirect("/login");
  }

  const { page, search } = await searchParams;

  const parsedPage = page ? parseInt(page, 10) : 1;

  const queryClient = getQueryClient();

  void queryClient.prefetchQuery(trpc.auth.getJwt.queryOptions());
  void queryClient.prefetchQuery(trpc.budgets.getOne.queryOptions());

  // Search query transactions
  void queryClient.prefetchQuery(
    trpc.transactions.getMany.queryOptions({
      page: parsedPage,
      pageSize: PAGE_SIZE,
      searchTerm: search,
    })
  );

  // All Transactions
  void queryClient.prefetchQuery(
    trpc.transactions.getMany.queryOptions({
      page: parsedPage,
      pageSize: PAGE_SIZE,
    })
  );

  return (
    <HydrateClient>
      <HomeView page={parsedPage || 1} />
    </HydrateClient>
  );
};

export default Page;
