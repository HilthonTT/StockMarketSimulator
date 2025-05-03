import NextAuth from "next-auth";

import authConfig from "./auth.config";

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

      return token;
    },

    async session({ session, token }) {
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
