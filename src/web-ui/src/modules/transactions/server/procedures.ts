import { z } from "zod";
import { TRPCError } from "@trpc/server";

import { createTRPCRouter, protectedProcedure } from "@/trpc/init";
import { fetchFromApi } from "@/lib/api";
import { PagedList } from "@/types";

import { TransactionResponse } from "../types";

export const transactionsRouter = createTRPCRouter({
  getMany: protectedProcedure
    .input(
      z.object({
        page: z.number().min(1),
        pageSize: z.number().max(100),
      })
    )
    .query(async ({ ctx, input }) => {
      const { userId, accessToken } = ctx;
      const { page, pageSize } = input;

      const pagedTransactions = await fetchFromApi<
        PagedList<TransactionResponse>
      >({
        accessToken,
        path: `/api/v1/users/${userId}/transactions`,
        queryParams: {
          page,
          pageSize,
        },
      });

      if (!pagedTransactions) {
        throw new TRPCError({ code: "NOT_FOUND" });
      }

      return pagedTransactions;
    }),
});
