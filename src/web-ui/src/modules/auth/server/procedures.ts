import { fetchFromApi } from "@/lib/api";
import {
  clearAuthCookies,
  refreshAccessTokenIfNeeded,
  setAuthCookies,
} from "@/lib/auth";
import {
  baseProcedure,
  createTRPCRouter,
  protectedProcedure,
} from "@/trpc/init";

import { LoginSchema, RegisterSchema } from "../schemas";
import { TokenResponse, UserResponse } from "../types";

export const authRouter = createTRPCRouter({
  refreshTokens: baseProcedure.mutation(async () => {
    const accessToken = await refreshAccessTokenIfNeeded();

    if (!accessToken) {
      return null;
    }

    try {
      const user = await fetchFromApi<UserResponse>({
        accessToken,
        path: "/api/v1/users/me",
      });

      return user;
    } catch {
      return null;
    }
  }),
  isAuthenticated: baseProcedure.query(async () => {
    const accessToken = await refreshAccessTokenIfNeeded();

    if (!accessToken) {
      return null;
    }

    try {
      const user = await fetchFromApi<UserResponse>({
        accessToken,
        path: "/api/v1/users/me",
      });

      return user;
    } catch {
      return null;
    }
  }),
  getJwt: protectedProcedure.query(async ({ ctx }) => {
    return ctx.accessToken;
  }),
  login: baseProcedure.input(LoginSchema).mutation(async ({ input }) => {
    const tokenResponse = await fetchFromApi<TokenResponse>({
      path: "/api/v1/users/login",
      method: "POST",
      body: input,
    });

    if (tokenResponse) {
      await setAuthCookies(tokenResponse);
    }

    return tokenResponse;
  }),
  register: baseProcedure.input(RegisterSchema).mutation(async ({ input }) => {
    const tokenResponse = await fetchFromApi<TokenResponse>({
      path: "/api/v1/users/register",
      method: "POST",
      body: input,
    });

    if (tokenResponse) {
      await setAuthCookies(tokenResponse);
    }

    return tokenResponse;
  }),
  logout: protectedProcedure.mutation(async ({ ctx }) => {
    const { accessToken, userId } = ctx;

    await Promise.all([
      clearAuthCookies(),
      fetchFromApi({
        accessToken,
        path: `/api/v1/users/${userId}/refresh-tokens`,
        method: "DELETE",
      }),
    ]);
  }),
});
