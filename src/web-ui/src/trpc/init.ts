import superjson from "superjson";
import https from "https";
import { cookies } from "next/headers";
import { jwtDecode } from "jwt-decode";
import { cache } from "react";

import { TokenResponse, UserResponse } from "@/modules/auth/types";

import { initTRPC, TRPCError } from "@trpc/server";
import { ACCESS_TOKEN, REFRESH_TOKEN } from "@/constants";
import { setAuthCookies } from "@/lib/auth";
import { fetchFromApi } from "@/lib/api";

export const agent = new https.Agent({
  rejectUnauthorized: false,
});

export const createTRPCContext = cache(async () => {
  const cookieStore = await cookies();
  const accessToken = cookieStore.get(ACCESS_TOKEN)?.value;
  const refreshToken = cookieStore.get(REFRESH_TOKEN)?.value;

  if (!accessToken) {
    return { userId: null, accessToken: null };
  }

  const decoded = jwtDecode(accessToken);

  if (!decoded || !decoded.sub || !decoded.exp) {
    return { userId: null, accessToken: null };
  }

  // Check if the access token is expired (decoded.exp is the expiration time in seconds)
  if (decoded.exp * 1000 < Date.now() && refreshToken) {
    try {
      const tokenResponse = await fetchFromApi<TokenResponse>({
        path: "/api/v1/users/refresh-tokens",
        method: "POST",
        body: refreshToken,
      });

      if (tokenResponse) {
        await setAuthCookies(tokenResponse);

        return { userId: decoded.sub, accessToken: tokenResponse.accessToken };
      }

      return { userId: null, accessToken: null };
    } catch (error) {
      console.error("Error refreshing token:", error);
      return { userId: null, accessToken: null };
    }
  }

  return { userId: decoded.sub, accessToken };
});

export type Context = Awaited<ReturnType<typeof createTRPCContext>>;

// Avoid exporting the entire t-object
// since it's not very descriptive.
// For instance, the use of a t variable
// is common in i18n libraries.
const t = initTRPC.context<Context>().create({
  /**
   * @see https://trpc.io/docs/server/data-transformers
   */
  transformer: superjson,
});

// Base router and procedure helpers
export const createTRPCRouter = t.router;
export const createCallerFactory = t.createCallerFactory;
export const baseProcedure = t.procedure;

export const protectedProcedure = t.procedure.use(async function isAuthed(
  opts
) {
  const { ctx } = opts;

  if (!ctx.userId) {
    throw new TRPCError({ code: "UNAUTHORIZED" });
  }

  const user = await fetchFromApi<UserResponse>({
    method: "GET",
    path: "/api/v1/users/me",
    accessToken: ctx.accessToken,
  });

  if (!user) {
    throw new TRPCError({ code: "UNAUTHORIZED" });
  }

  // Return the updated context with the user object
  return opts.next({
    ctx: {
      ...ctx,
      user,
    },
  });
});
