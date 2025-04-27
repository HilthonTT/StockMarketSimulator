import { HomeView } from "@/modules/home/ui/views/home-view";

import { HydrateClient, trpc } from "@/trpc/server";

const Page = () => {
  void trpc.stocks.getMany.prefetch();

  return (
    <HydrateClient>
      <HomeView />
    </HydrateClient>
  );
};

export default Page;
