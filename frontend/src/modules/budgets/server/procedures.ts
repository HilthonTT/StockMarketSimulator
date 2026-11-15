import { TRPCError } from "@trpc/server";

import { createTRPCRouter, protectedProcedure } from "@/trpc/init";
import { fetchFromApi } from "@/lib/api";

import { BudgetResponse } from "../types";

export const budgetsRouter = createTRPCRouter({
  getOne: protectedProcedure.query(async ({ ctx }) => {
    const { user, accessToken } = ctx;

    const budgetResponse = await fetchFromApi<BudgetResponse>({
      accessToken,
      path: `/api/v1/users/${user.id}/budget`,
    });

    if (!budgetResponse) {
      throw new TRPCError({ code: "NOT_FOUND" });
    }

    return budgetResponse;
  }),

  /** Back to $5,000 cash: positions, trade history and open orders are wiped. */
  reset: protectedProcedure.mutation(async ({ ctx }) => {
    const { user, accessToken } = ctx;

    await fetchFromApi({
      accessToken,
      method: "POST",
      path: `/api/v1/users/${user.id}/budget/reset`,
      throwOnNotFound: true,
    });

    return { ok: true };
  }),
});
