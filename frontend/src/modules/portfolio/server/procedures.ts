import { fetchFromApi } from "@/lib/api";
import { createTRPCRouter, protectedProcedure } from "@/trpc/init";

import type { PositionResponse } from "../types";

export const portfolioRouter = createTRPCRouter({
  getPositions: protectedProcedure.query(async ({ ctx }) => {
    const { user, accessToken } = ctx;

    const positions = await fetchFromApi<PositionResponse[]>({
      accessToken,
      path: `/api/v1/users/${user.id}/positions`,
    });

    return (positions ?? [])
      .filter((position) => position.quantity > 0)
      .map((position) => ({
        ...position,
        ticker: position.ticker.toUpperCase(),
      }))
      .sort((a, b) => b.totalCost - a.totalCost);
  }),
});
