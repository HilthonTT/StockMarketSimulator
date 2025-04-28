import fetch from "node-fetch";
import { z } from "zod";

import { createTRPCRouter, protectedProcedure, agent } from "@/trpc/init";
import { SERVER_URL } from "@/constants";
import { TRPCError } from "@trpc/server";
import { PagedList, ProblemDetails } from "@/types";

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

      const url = new URL(`${SERVER_URL}/api/v1/users/${userId}/transactions`);
      url.searchParams.append("page", page.toString());
      url.searchParams.append("pageSize", pageSize.toString());

      const response = await fetch(url.toString(), {
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

      const pagedTransactions =
        (await response.json()) as PagedList<TransactionResponse>;

      return pagedTransactions;
    }),
});
