import { TRPCError } from "@trpc/server";

import type { UserResponse } from "@/modules/auth/types";
import type { PurchasedStockTickersResponse } from "@/modules/stocks/types";

import { fetchFromApi } from "@/lib/api";
import { createTRPCRouter, protectedProcedure } from "@/trpc/init";

import { UpdatePasswordSchema, UpdateUsernameSchema } from "../schemas";

export const usersRouter = createTRPCRouter({
  getCurrent: protectedProcedure.query(async ({ ctx }) => {
    const { accessToken } = ctx;

    const userResponse = await fetchFromApi<UserResponse>({
      path: `/api/v1/users/me`,
      accessToken,
    });

    if (!userResponse) {
      throw new TRPCError({ code: "NOT_FOUND" });
    }

    return userResponse;
  }),
  updateUsername: protectedProcedure
    .input(UpdateUsernameSchema)
    .mutation(async ({ input, ctx }) => {
      const { accessToken, user } = ctx;

      await fetchFromApi({
        method: "PATCH",
        accessToken,
        path: `/api/v1/users/${user.id}`,
        body: {
          username: input.newUsername,
        },
      });

      return true;
    }),
  updatePassword: protectedProcedure
    .input(UpdatePasswordSchema)
    .mutation(async ({ input, ctx }) => {
      const { accessToken, user } = ctx;

      await fetchFromApi({
        method: "POST",
        accessToken,
        path: `/api/v1/users/${user.id}/change-password`,
        body: {
          currentPassword: input.currentPassword,
          newPassword: input.newPassword,
        },
      });

      return true;
    }),
  getPurchasedStockTickers: protectedProcedure.query(async ({ ctx }) => {
    const { accessToken, user } = ctx;

    const purchasedStockTickersResponse =
      await fetchFromApi<PurchasedStockTickersResponse>({
        path: `/api/v1/users/${user.id}/purchased-stock-tickers`,
        accessToken,
      });

    if (!purchasedStockTickersResponse) {
      throw new TRPCError({ code: "NOT_FOUND" });
    }

    return purchasedStockTickersResponse;
  }),
});
