import type { Metadata } from "next";

import { LEADERBOARD_SIZE } from "@/modules/leaderboard/constants";
import { LeaderboardView } from "@/modules/leaderboard/ui/views/leaderboard-view";

import { getQueryClient, HydrateClient, trpc } from "@/trpc/server";

export const metadata: Metadata = { title: "Leaderboard" };

const Page = async () => {
  // The view suspends on this query; without a server prefetch, SSR would call the API unauthenticated.
  await getQueryClient().prefetchQuery(trpc.leaderboard.get.queryOptions({ limit: LEADERBOARD_SIZE }));

  return (
    <HydrateClient>
      <LeaderboardView />
    </HydrateClient>
  );
};

export default Page;
