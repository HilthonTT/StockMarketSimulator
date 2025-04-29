import { redirect } from "next/navigation";

import { HomeView } from "@/modules/home/ui/views/home-view";

import { HydrateClient, trpc } from "@/trpc/server";
import { PAGE_SIZE } from "@/constants";

interface PageProps {
  searchParams: Promise<{
    page?: number;
  }>;
}

const Page = async ({ searchParams }: PageProps) => {
  const user = await trpc.auth.isAuthenticated();
  if (!user) {
    return redirect("/login");
  }

  const { page } = await searchParams;

  void trpc.auth.getJwt.prefetch();
  void trpc.budgets.getOne.prefetch();
  void trpc.transactions.getMany.prefetch({
    page: page || 1,
    pageSize: PAGE_SIZE,
  });

  return (
    <HydrateClient>
      <HomeView page={page || 1} />
    </HydrateClient>
  );
};

export default Page;
