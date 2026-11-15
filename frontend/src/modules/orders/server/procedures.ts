import { z } from "zod";
import { v4 as uuidv4 } from "uuid";

import { IDEMPOTENCY_HEADER } from "@/modules/transactions/constants";

import { createTRPCRouter, protectedProcedure } from "@/trpc/init";
import { fetchFromApi } from "@/lib/api";

import type { OrderResponse } from "../types";
import { PlaceOrderSchema } from "../schemas";

export const ordersRouter = createTRPCRouter({
  getMany: protectedProcedure
    .input(z.object({ status: z.enum(["open", "all"]).default("all") }))
    .query(async ({ ctx, input }) => {
      const { user, accessToken } = ctx;

      const orders = await fetchFromApi<OrderResponse[]>({
        accessToken,
        path: `/api/v1/users/${user.id}/orders`,
        queryParams: { status: input.status },
      });

      return (orders ?? []).map((order) => ({ ...order, ticker: order.ticker.toUpperCase() }));
    }),

  place: protectedProcedure.input(PlaceOrderSchema).mutation(async ({ ctx, input }) => {
    const { accessToken, user } = ctx;
    const ticker = input.ticker.toUpperCase();

    const id = await fetchFromApi<string>({
      accessToken,
      method: "POST",
      path: "/api/v1/orders",
      headers: { [IDEMPOTENCY_HEADER]: uuidv4() },
      body: { ...input, ticker, userId: user.id },
      throwOnNotFound: true,
    });

    return { id, ...input, ticker };
  }),

  cancel: protectedProcedure
    .input(z.object({ orderId: z.string().uuid() }))
    .mutation(async ({ ctx, input }) => {
      await fetchFromApi({
        accessToken: ctx.accessToken,
        method: "DELETE",
        path: `/api/v1/orders/${input.orderId}`,
        throwOnNotFound: true,
      });

      return { orderId: input.orderId };
    }),
});
