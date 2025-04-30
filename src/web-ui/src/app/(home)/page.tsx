import { redirect } from "next/navigation";

import { HomeView } from "@/modules/home/ui/views/home-view";

import { HydrateClient, trpc } from "@/trpc/server";
import { PAGE_SIZE } from "@/constants";

interface PageProps {
  searchParams: Promise<{
    page?: string;
    search?: string;
  }>;
}

const Page = async ({ searchParams }: PageProps) => {
  const user = await trpc.auth.isAuthenticated();
  if (!user) {
    return redirect("/login");
  }

  const { page, search } = await searchParams;

  const parsedPage = page ? parseInt(page, 10) : 1;

  void trpc.auth.getJwt.prefetch();
  void trpc.budgets.getOne.prefetch();
  void trpc.transactions.getMany.prefetch({
    page: parsedPage,
    pageSize: PAGE_SIZE,
    searchTerm: search,
  });

  return (
    <HydrateClient>
      <HomeView page={parsedPage || 1} />
    </HydrateClient>
  );
};

export default Page;
