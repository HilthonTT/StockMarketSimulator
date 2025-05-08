import { TRPCError } from "@trpc/server";

import { z } from "zod";
import { createTRPCRouter, baseProcedure } from "@/trpc/init";
import { fetchFromApi } from "@/lib/api";

import type { ShortenUrlResponse } from "../types";

export const shortenRouter = createTRPCRouter({
  getShortenUrl: baseProcedure
    .input(
      z.object({
        ticker: z.string().max(10),
      })
    )
    .query(async ({ input }) => {
      const { ticker } = input;

      const shortenUrlResponse = await fetchFromApi<ShortenUrlResponse>({
        path: `/api/v1/shorten/ticker/${ticker}`,
      });

      if (!shortenUrlResponse) {
        throw new TRPCError({ code: "NOT_FOUND" });
      }

      return shortenUrlResponse;
    }),
});
