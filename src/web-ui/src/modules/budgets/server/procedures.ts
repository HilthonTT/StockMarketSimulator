import fetch from "node-fetch";

import { createTRPCRouter, protectedProcedure, agent } from "@/trpc/init";
import { SERVER_URL } from "@/constants";
import { TRPCError } from "@trpc/server";
import { ProblemDetails } from "@/types";

import { BudgetResponse } from "../types";

export const budgetsRouter = createTRPCRouter({
  getOne: protectedProcedure.query(async ({ ctx }) => {
    const { userId, accessToken } = ctx;

    const url = `${SERVER_URL}/api/v1/users/${userId}/budget`;

    const response = await fetch(url, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${accessToken}`,
      },
      agent,
    });

    if (!response.ok) {
      const problemDetails = (await response.json()) as ProblemDetails;

      throw new TRPCError({
        code: "BAD_REQUEST",
        message: `Error: ${problemDetails.title} - ${problemDetails.detail}`,
      });
    }

    const budgetResponse = (await response.json()) as BudgetResponse;

    return budgetResponse;
  }),
});
