import NextAuth from "next-auth";
import { jwtDecode } from "jwt-decode";

import authConfig from "./auth.config";
import { JWT_INVALID_ERROR } from "./constants";

function getExpiry(accessToken: string | undefined) {
  if (!accessToken) {
    return undefined;
  }

  try {
    return jwtDecode<{ exp?: number }>(accessToken).exp;
  } catch {
    return undefined;
  }
}

export const { handlers, signIn, signOut, auth } = NextAuth({
  pages: {
    signIn: "/login",
    error: "/login",
  },
  callbacks: {
    async jwt({ token, user }) {
      const now = Math.floor(Date.now() / 1000);

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

      const exp = getExpiry(token.accessToken);

      // No API token (e.g. an OAuth sign-in that never reached the API) or an expired one.
      if (!exp || exp < now) {
        return null;
      }

      return token;
    },

    session({ session, token }) {
      const now = Math.floor(Date.now() / 1000);

      const exp = getExpiry(token.accessToken);

      session.accessToken = token.accessToken;
      session.refreshToken = token.refreshToken;
      session.expiresAt = token.expiresAt;
      session.user = {
        ...session.user,
        id: token.userId,
      };

      if (!exp || exp < now) {
        session.error = JWT_INVALID_ERROR;
      }

      return session;
    },
  },
  session: { strategy: "jwt" },
  ...authConfig,
});

export { auth as middleware };
