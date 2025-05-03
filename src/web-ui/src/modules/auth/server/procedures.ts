import { fetchFromApi } from "@/lib/api";
import {
  baseProcedure,
  createTRPCRouter,
  protectedProcedure,
} from "@/trpc/init";

import { auth, signIn, signOut } from "../auth";
import { LoginSchema, RegisterSchema } from "../schemas";
import { TokenResponse } from "../types";
import { DEFAULT_LOGIN_REDIRECT } from "../routes";

export const authRouter = createTRPCRouter({
  current: baseProcedure.query(async () => {
    const session = await auth();

    return session?.user;
  }),
  getJwt: protectedProcedure.query(async ({ ctx }) => {
    return ctx.accessToken;
  }),
  login: baseProcedure.input(LoginSchema).mutation(async ({ input }) => {
    await signIn("credentials", {
      email: input.email,
      password: input.password,
      redirectTo: DEFAULT_LOGIN_REDIRECT,
    });
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
