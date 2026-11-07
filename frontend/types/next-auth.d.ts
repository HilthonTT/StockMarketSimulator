import { type DefaultSession } from "next-auth";
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { type JWT } from "next-auth/jwt";

export type ExtendedUser = DefaultSession["user"] & {
  id: string;
};

declare module "next-auth" {
  interface Session {
    user: ExtendedUser;
    accessToken: string;
    refreshToken?: string;
    expiresAt?: number;
    error?: string;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    accessToken: string;
    refreshToken?: string;
    expiresAt?: number;
    userId: string;
  }
}
