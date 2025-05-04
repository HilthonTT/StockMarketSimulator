import NextAuth from "next-auth";
import { jwtDecode } from "jwt-decode";

import authConfig from "./auth.config";
import { JWT_INVALID_ERROR } from "./constants";

export const { handlers, signIn, signOut, auth } = NextAuth({
  pages: {
    signIn: "/login",
    error: "/error",
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

      const decoded = jwtDecode<{ exp?: number }>(token.accessToken);

      if (decoded.exp && decoded.exp < now) {
        return null;
      }

      return token;
    },

    session({ session, token }) {
      const now = Math.floor(Date.now() / 1000);

      const decoded = jwtDecode<{ exp?: number }>(token.accessToken);

      session.accessToken = token.accessToken;
      session.refreshToken = token.refreshToken;
      session.expiresAt = token.expiresAt;
      session.user = {
        ...session.user,
        id: token.userId,
      };

      if (decoded.exp && decoded.exp < now) {
        session.error = JWT_INVALID_ERROR;
      }

      return session;
    },
  },
  session: { strategy: "jwt" },
  ...authConfig,
});

export { auth as middleware };
