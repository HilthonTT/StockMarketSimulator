import Credentials from "next-auth/providers/credentials";
import type { NextAuthConfig } from "next-auth";
import { jwtDecode } from "jwt-decode";

import { fetchFromApi } from "@/lib/api";

import { LoginSchema } from "./schemas";
import { TokenResponse, UserResponse } from "./types";

export default {
  secret: process.env.AUTH_SECRET,
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
