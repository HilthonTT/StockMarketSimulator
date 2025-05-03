import Credentials from "next-auth/providers/credentials";
import Github from "next-auth/providers/github";
import Google from "next-auth/providers/google";
import type { NextAuthConfig } from "next-auth";
import { jwtDecode } from "jwt-decode";

import { fetchFromApi } from "@/lib/api";

import { LoginSchema } from "./schemas";
import { TokenResponse, UserResponse } from "./types";

export default {
  secret: process.env.AUTH_SECRET,
  providers: [
    Google({}),
    Github({}),
    Credentials({
      async authorize(credentials) {
        const validatedFields = await LoginSchema.safeParseAsync(credentials);

        if (!validatedFields.success) {
          return null;
        }

        const { email, password } = validatedFields.data;

        const tokenResponse = await fetchFromApi<TokenResponse>({
          method: "POST",
          path: "/api/v1/users/login",
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
