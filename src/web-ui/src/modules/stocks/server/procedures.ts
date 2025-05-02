import { fetchFromApi } from "@/lib/api";
import { z } from "zod";
import { createTRPCRouter, protectedProcedure } from "@/trpc/init";

import { StockPriceResponse } from "../types";
import { TRPCError } from "@trpc/server";

export const stocksRouter = createTRPCRouter({
  getOne: protectedProcedure
    .input(
      z.object({
        ticker: z.string().min(1).max(10),
      })
    )
    .query(async ({ ctx, input }) => {
      const { accessToken } = ctx;
      const { ticker } = input;

      const stockPriceResponse = await fetchFromApi<StockPriceResponse>({
        path: `/api/v1/stocks/${ticker}`,
        accessToken,
      });

      if (!stockPriceResponse) {
        throw new TRPCError({ code: "NOT_FOUND" });
      }

      return stockPriceResponse;
    }),
});
