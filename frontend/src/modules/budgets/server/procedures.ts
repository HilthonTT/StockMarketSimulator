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
});
