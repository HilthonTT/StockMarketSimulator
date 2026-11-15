"use server";

import { AuthError } from "next-auth";
import { unstable_rethrow } from "next/navigation";
import { z } from "zod";

import { fetchFromApi } from "@/lib/api";

import { LoginSchema } from "../schemas";
import { auth, signIn, signOut } from "../auth";
import { DEFAULT_LOGIN_REDIRECT } from "../routes";

export type LoginResult = { error: string } | undefined;

export const login = async (
  values: z.infer<typeof LoginSchema>,
  callbackUrl?: string | null
): Promise<LoginResult> => {
  const validatedFields = await LoginSchema.safeParseAsync(values);

  if (!validatedFields.success) {
    return { error: "Please enter a valid email and password." };
  }

  const { email, password } = validatedFields.data;

  // Only allow same-site relative redirects.
  const redirectTo =
    callbackUrl && callbackUrl.startsWith("/") && !callbackUrl.startsWith("//")
      ? callbackUrl
      : DEFAULT_LOGIN_REDIRECT;

  try {
    // Throws NEXT_REDIRECT on success, which Next turns into a client navigation.
    await signIn("credentials", { email, password, redirectTo });
  } catch (error) {
    if (error instanceof AuthError) {
      if (error.type === "CredentialsSignin") {
        return { error: "Invalid email or password." };
      }

      // authorize() throws when the API can't be reached (fetchFromApi raises a TRPCError).
      if (error.type === "CallbackRouteError") {
        return {
          error: "The trading server is unreachable right now. Make sure the API is running, then try again.",
        };
      }

      return { error: "We couldn't sign you in. Please try again." };
    }

    // NEXT_REDIRECT (a successful sign-in) must propagate; anything else becomes a friendly message.
    unstable_rethrow(error);

    console.error("Login failed:", error);
    return { error: "We couldn't reach the trading server. Please try again in a moment." };
  }
};

/** Revokes the API refresh tokens for this user, then clears the session. */
export const logout = async () => {
  const session = await auth();

  if (session?.user?.id && session.accessToken) {
    await fetchFromApi({
      accessToken: session.accessToken,
      path: `/api/v1/authentication/${session.user.id}/refresh-tokens`,
      method: "DELETE",
    }).catch(() => {
      // Signing out locally must still work when the API is unreachable.
    });
  }

  await signOut({ redirectTo: "/login" });
};
