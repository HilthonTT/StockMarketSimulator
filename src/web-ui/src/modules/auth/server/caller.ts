import { getDecodedTokenFromCookies } from "@/lib/auth";
import { appRouter } from "@/trpc/routers/_app";

import { UserResponse } from "../types";

export async function fetchUser(): Promise<UserResponse | null> {
  const tokenResponse = await getDecodedTokenFromCookies();

  const caller = appRouter.createCaller({
    userId: tokenResponse.userId,
    accessToken: tokenResponse.accessToken,
  });

  const user = caller.auth.refreshTokens();

  return user;
}
