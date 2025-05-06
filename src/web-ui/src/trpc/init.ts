import superjson from "superjson";
import { cache } from "react";

import { UserResponse } from "@/modules/auth/types";
import { auth } from "@/modules/auth/auth";

import { initTRPC, TRPCError } from "@trpc/server";
import { fetchFromApi } from "@/lib/api";
import { ratelimit } from "@/lib/ratelimit";

export const createTRPCContext = cache(async () => {
  const session = await auth();

  return {
    user: session?.user ?? null,
    ...session,
  };
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

  if (!ctx.user) {
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

  const { success } = await ratelimit.limit(user.id);

  if (!success) {
    throw new TRPCError({ code: "TOO_MANY_REQUESTS" });
  }

  // Return the updated context with the user object
  return opts.next({
    ctx: {
      ...ctx,
      user,
    },
  });
});
