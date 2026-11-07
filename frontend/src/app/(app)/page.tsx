import type { Metadata } from "next";

import { DashboardView } from "@/modules/dashboard/ui/views/dashboard-view";
import { RECENT_ACTIVITY_SIZE } from "@/modules/dashboard/constants";

import { getQueryClient, HydrateClient, trpc } from "@/trpc/server";

export const metadata: Metadata = { title: "Dashboard" };

const Page = async () => {
  const queryClient = getQueryClient();

  void queryClient.prefetchQuery(
    trpc.transactions.getMany.queryOptions({ pageSize: RECENT_ACTIVITY_SIZE })
  );

  return (
    <HydrateClient>
      <DashboardView />
    </HydrateClient>
  );
};

export default Page;
