import Credentials from "next-auth/providers/credentials";
import type { NextAuthConfig } from "next-auth";
import { jwtDecode } from "jwt-decode";

import { fetchFromApi } from "@/lib/api";

import { LoginSchema } from "./schemas";
import { TokenResponse, UserResponse } from "./types";

const DEV_AUTH_SECRET = "dev-only-stock-market-simulator-secret-change-me-in-production";

/**
 * Production must provide AUTH_SECRET. In development a fixed, publicly known secret is used
 * so the app runs without a .env file; it must never protect real sessions.
 */
function resolveAuthSecret() {
  const secret = process.env.AUTH_SECRET;

  if (secret || process.env.NODE_ENV === "production") {
    return secret;
  }

  if (!globalThis.__smsDevSecretWarned) {
    globalThis.__smsDevSecretWarned = true;
    console.warn("[auth] AUTH_SECRET is not set; using the development-only fallback secret.");
  }

  return DEV_AUTH_SECRET;
}

declare global {
  var __smsDevSecretWarned: boolean | undefined;
}

export default {
  secret: resolveAuthSecret(),
  // Behind localhost/dev proxies the host header is trusted; production sets AUTH_TRUST_HOST/AUTH_URL.
  trustHost: process.env.NODE_ENV !== "production" || process.env.AUTH_TRUST_HOST === "true",
  // Only credentials: sessions need an API access token, which OAuth sign-ins never got.
  providers: [
    Credentials({
      async authorize(credentials) {
        const validatedFields = await LoginSchema.safeParseAsync(credentials);

        if (!validatedFields.success) {
          return null;
        }

        const { email, password } = validatedFields.data;

        const tokenResponse = await fetchFromApi<TokenResponse>({
          method: "POST",
          path: "/api/v1/authentication/login",
          body: { email, password },
        });

        if (!tokenResponse) {
          return null;
        }

        const { sub } = jwtDecode(tokenResponse.accessToken);

        if (!sub) {
          return null;
        }

        const user = await fetchFromApi<UserResponse>({
          method: "GET",
          accessToken: tokenResponse.accessToken,
          path: "/api/v1/users/me",
        });

        if (!user) {
          return null;
        }

        return { ...user, ...tokenResponse };
      },
    }),
  ],
} satisfies NextAuthConfig;
