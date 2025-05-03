import NextAuth from "next-auth";

import authConfig from "./auth.config";
import { fetchFromApi } from "@/lib/api";

import { TokenResponse } from "./types";

export const { handlers, signIn, signOut, auth } = NextAuth({
  pages: {
    signIn: "/login",
    error: "/error",
  },
  callbacks: {
    async jwt({ token, user }) {
      if (user) {
        const extendedUser = user as typeof user & {
          accessToken: string;
          refreshToken?: string;
          expiresAt?: number;
          id: string;
        };

        token.accessToken = extendedUser.accessToken;
        token.refreshToken = extendedUser.refreshToken;
        token.expiresAt = extendedUser.expiresAt;
        token.userId = extendedUser.id;
      }

      // current time in seconds
      const now = Math.floor(Date.now() / 1000);

      if (token.expiresAt && token.expiresAt < now) {
        const tokenResponse = await fetchFromApi<TokenResponse>({
          path: `/api/v1/users/${token.userId}/refresh-tokens`,
          method: "POST",
          body: { refreshToken: token.refreshToken },
        });

        if (!tokenResponse) {
          return null;
        }

        token.accessToken = tokenResponse?.accessToken;
        token.refreshToken = tokenResponse?.refreshToken;
      }

      return token;
    },

    session({ session, token }) {
      session.accessToken = token.accessToken;
      session.refreshToken = token.refreshToken;
      session.expiresAt = token.expiresAt;
      session.user = {
        ...session.user,
        id: token.userId,
      };

      return session;
    },
  },
  session: { strategy: "jwt" },
  ...authConfig,
});

export { auth as middleware };
