import { TRPCError } from "@trpc/server";

import type { UserResponse } from "@/modules/auth/types";

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
    .mutation(async ({ input }) => {
      console.log({ input });
      // TODO: Do this later
    }),
  updatePassword: protectedProcedure
    .input(UpdatePasswordSchema)
    .mutation(async ({ input }) => {
      console.log({ input });
      // TODO: Do this later
    }),
});
