import { z } from "zod";
import { TRPCError } from "@trpc/server";
import { v4 as uuidv4 } from "uuid";

import { createTRPCRouter, protectedProcedure } from "@/trpc/init";
import { fetchFromApi } from "@/lib/api";
import { CursorResponse } from "@/types";

import { TransactionCountResponse, TransactionResponse } from "../types";
import { BuyTransactionSchema, SellTransactionSchema } from "../schemas";
import { IDEMPOTENCY_HEADER } from "../constants";

export const transactionsRouter = createTRPCRouter({
  getCount: protectedProcedure.query(async ({ ctx }) => {
    const { user, accessToken } = ctx;

    const transactionCountResponse =
      await fetchFromApi<TransactionCountResponse>({
        accessToken,
        path: `/api/v1/users/${user.id}/transaction-count`,
      });

    if (!transactionCountResponse) {
      throw new TRPCError({ code: "NOT_FOUND" });
    }

    return transactionCountResponse;
  }),
  getMany: protectedProcedure
    .input(
      z.object({
        searchTerm: z.string().optional(),
        startDate: z.date().optional(),
        endDate: z.date().optional(),
        pageSize: z.number().max(100),
        cursor: z.string().uuid().optional(),
      })
    )
    .query(async ({ ctx, input }) => {
      const { user, accessToken } = ctx;
      const { cursor, pageSize, searchTerm, startDate, endDate } = input;

      const cursorResponse = await fetchFromApi<
        CursorResponse<TransactionResponse>
      >({
        accessToken,
        path: `/api/v1/users/${user.id}/transactions`,
        queryParams: {
          cursor,
          pageSize,
          searchTerm,
          startDate:
            startDate?.getTime() === new Date(0).getTime()
              ? undefined
              : startDate,
          endDate:
            endDate?.getTime() === new Date(0).getTime() ? undefined : endDate,
        },
      });

      if (!cursorResponse) {
        throw new TRPCError({ code: "NOT_FOUND" });
      }

      return cursorResponse;
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
