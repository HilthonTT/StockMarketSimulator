import { fetchFromApi } from "@/lib/api";
import {
  baseProcedure,
  createTRPCRouter,
  protectedProcedure,
} from "@/trpc/init";

import { auth, signOut } from "../auth";
import { RegisterSchema } from "../schemas";
import { TokenResponse } from "../types";

export const authRouter = createTRPCRouter({
  current: baseProcedure.query(async () => {
    const session = await auth();

    return session?.user;
  }),
  getJwt: protectedProcedure.query(async ({ ctx }) => {
    return ctx.accessToken;
  }),
  register: baseProcedure.input(RegisterSchema).mutation(async ({ input }) => {
    const tokenResponse = await fetchFromApi<TokenResponse>({
      path: "/api/v1/users/register",
      method: "POST",
      body: input,
    });

    return tokenResponse;
  }),
  logout: protectedProcedure.mutation(async ({ ctx }) => {
    const { accessToken, user } = ctx;

    await Promise.all([
      signOut(),
      fetchFromApi({
        accessToken,
        path: `/api/v1/users/${user.id}/refresh-tokens`,
        method: "DELETE",
      }),
    ]);
  }),
});
