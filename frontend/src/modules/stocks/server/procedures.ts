import { z } from "zod";

import { fetchFromApi } from "@/lib/api";
import { createTRPCRouter, protectedProcedure } from "@/trpc/init";
import type { PagedList } from "@/types";

import type { StockPriceResponse, StockSearchResponse } from "../types";

const tickerSchema = z
  .string()
  .trim()
  .min(1)
  .max(10)
  .transform((ticker) => ticker.toUpperCase());

export const stocksRouter = createTRPCRouter({
  /** Latest simulated quote, or `null` when the ticker is unknown. */
  getOne: protectedProcedure
    .input(z.object({ ticker: tickerSchema }))
    .query(async ({ ctx, input }) => {
      const stockPriceResponse = await fetchFromApi<StockPriceResponse>({
        path: `/api/v1/stocks/${encodeURIComponent(input.ticker)}`,
        accessToken: ctx.accessToken,
      });

      if (!stockPriceResponse) {
        return null;
      }

      return {
        ...stockPriceResponse,
        ticker: stockPriceResponse.ticker.toUpperCase(),
      };
    }),
  search: protectedProcedure
    .input(
      z.object({
        searchTerm: z.string().trim().max(50).optional(),
        /** Page number; named `cursor` so the procedure works with infinite queries. */
        cursor: z.number().int().min(1).optional(),
        pageSize: z.number().int().min(1).max(100).default(20),
      })
    )
    .query(async ({ ctx, input }) => {
      const page = input.cursor ?? 1;

      const result = await fetchFromApi<PagedList<StockSearchResponse>>({
        path: `/api/v1/stocks/search`,
        accessToken: ctx.accessToken,
        queryParams: {
          searchTerm: input.searchTerm || undefined,
          page,
          pageSize: input.pageSize,
        },
      });

      return (
        result ?? {
          items: [],
          page,
          pageSize: input.pageSize,
          totalCount: 0,
          hasNextPage: false,
          hasPreviousPage: false,
        }
      );
    }),
  getTopPerformer: protectedProcedure.query(async ({ ctx }) => {
    const { accessToken, user } = ctx;

    const stockPriceResponse = await fetchFromApi<StockPriceResponse>({
      path: `/api/v1/stocks/top-performer`,
      accessToken,
      headers: {
        userId: user.id,
      },
    });

    return stockPriceResponse ?? null;
  }),
});
