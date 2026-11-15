import { z } from "zod";

import { createTRPCRouter, protectedProcedure } from "@/trpc/init";
import { fetchFromApi } from "@/lib/api";

import type { LeaderboardResponse } from "../types";

export const leaderboardRouter = createTRPCRouter({
  get: protectedProcedure
    .input(z.object({ limit: z.number().int().min(1).max(100).default(25) }))
    .query(async ({ ctx, input }) => {
      const leaderboard = await fetchFromApi<LeaderboardResponse>({
        accessToken: ctx.accessToken,
        path: "/api/v1/leaderboard",
        queryParams: { limit: input.limit },
      });

      return leaderboard ?? { entries: [], currentUser: null };
    }),
});
