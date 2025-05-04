import { z } from "zod";
import { TRPCError } from "@trpc/server";
import { v4 as uuidv4 } from "uuid";

import { createTRPCRouter, protectedProcedure } from "@/trpc/init";
import { fetchFromApi } from "@/lib/api";
import { PagedList } from "@/types";

import { TransactionResponse } from "../types";
import { BuyTransactionSchema, SellTransactionSchema } from "../schemas";
import { IDEMPOTENCY_HEADER } from "../constants";

export const transactionsRouter = createTRPCRouter({
  getMany: protectedProcedure
    .input(
      z.object({
        searchTerm: z.string().optional(),
        startDate: z.date().optional(),
        endDate: z.date().optional(),
        page: z.number().min(1),
        pageSize: z.number().max(100),
      })
    )
    .query(async ({ ctx, input }) => {
      const { user, accessToken } = ctx;
      const { page, pageSize, searchTerm, startDate, endDate } = input;

      const pagedTransactions = await fetchFromApi<
        PagedList<TransactionResponse>
      >({
        accessToken,
        path: `/api/v1/users/${user.id}/transactions`,
        queryParams: {
          page,
          pageSize,
          searchTerm,
          startDate,
          endDate,
        },
      });

      if (!pagedTransactions) {
        throw new TRPCError({ code: "NOT_FOUND" });
      }

      return pagedTransactions;
    }),

  buy: protectedProcedure
    .input(BuyTransactionSchema)
    .mutation(async ({ ctx, input }) => {
      const { accessToken, user } = ctx;
      const { ticker, quantity } = input;

      await fetchFromApi({
        accessToken,
        method: "POST",
        path: "/api/v1/transactions/buy",
        headers: {
          [IDEMPOTENCY_HEADER]: uuidv4(),
        },
        body: { ticker, quantity, userId: user.id },
      });
    }),

  sell: protectedProcedure
    .input(SellTransactionSchema)
    .mutation(async ({ ctx, input }) => {
      const { accessToken, user } = ctx;
      const { ticker, quantity } = input;

      await fetchFromApi({
        accessToken,
        method: "POST",
        path: "/api/v1/transactions/sell",
        headers: {
          [IDEMPOTENCY_HEADER]: uuidv4(),
        },
        body: { ticker, quantity, userId: user.id },
      });
    }),
});
