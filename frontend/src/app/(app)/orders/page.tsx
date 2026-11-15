import type { Metadata } from "next";

import { OrdersView } from "@/modules/orders/ui/views/orders-view";

import { getQueryClient, HydrateClient, trpc } from "@/trpc/server";

export const metadata: Metadata = { title: "Orders" };

const Page = async () => {
  // The view suspends on this query; without a server prefetch, SSR would call the API unauthenticated.
  await getQueryClient().prefetchQuery(trpc.orders.getMany.queryOptions({ status: "all" }));

  return (
    <HydrateClient>
      <OrdersView />
    </HydrateClient>
  );
};

export default Page;
