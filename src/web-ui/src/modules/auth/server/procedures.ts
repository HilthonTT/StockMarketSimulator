import fetch from "node-fetch";
import { cookies } from "next/headers";
import { TRPCError } from "@trpc/server";
import { jwtDecode } from "jwt-decode";

import { baseProcedure, createTRPCRouter, agent } from "@/trpc/init";
import { ACCESS_TOKEN, REFRESH_TOKEN, SERVER_URL } from "@/constants";
import { ProblemDetails } from "@/types";

import { LoginSchema, RegisterSchema } from "../schemas";
import { TokenResponse, UserResponse } from "../types";

export const authRouter = createTRPCRouter({
  isAuthenticated: baseProcedure.query(async () => {
    const url = `${SERVER_URL}/api/v1/users/me`;

    const cookieStore = await cookies();
    let accessToken = cookieStore.get(ACCESS_TOKEN)?.value;
    let refreshToken = cookieStore.get(REFRESH_TOKEN)?.value;

    if (!accessToken) {
      return null;
    }

    const decoded = jwtDecode(accessToken);

    if (!decoded || !decoded.sub || !decoded.exp) {
      return null;
    }

    // Check if the access token is expired (decoded.exp is the expiration time in seconds)
    if (decoded.exp * 1000 < Date.now() && refreshToken) {
      const response = await fetch(
        `${SERVER_URL}/api/v1/users/refresh-tokens`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({ refreshToken }),
          agent,
        }
      );

      if (response.ok) {
        const tokenResponse = (await response.json()) as TokenResponse;

        const cookieStore = await cookies();
        cookieStore.set(ACCESS_TOKEN, tokenResponse.accessToken, {
          httpOnly: true,
          secure: process.env.NODE_ENV === "production",
          path: "/",
          sameSite: "lax",
          maxAge: 60 * 60 * 24 * 1, // 1 day
        });

        cookieStore.set(REFRESH_TOKEN, tokenResponse.refreshToken, {
          httpOnly: true,
          secure: process.env.NODE_ENV === "production",
          path: "/",
          sameSite: "lax",
          maxAge: 60 * 60 * 24 * 7, // 7 days
        });

        accessToken = tokenResponse.accessToken;
        refreshToken = tokenResponse.refreshToken;
      } else {
        // Refresh failed, clean up tokens
        cookieStore.set(ACCESS_TOKEN, "", { maxAge: -1, path: "/" });
        cookieStore.set(REFRESH_TOKEN, "", { maxAge: -1, path: "/" });
        return null;
      }
    }

    const response = await fetch(url, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${accessToken}`,
      },
      agent,
    });

    if (response.ok) {
      const user = (await response.json()) as UserResponse;

      return user;
    }

    return null;
  }),
  login: baseProcedure.input(LoginSchema).mutation(async ({ input }) => {
    const url = `${SERVER_URL}/api/v1/users/login`;

    const response = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(input),
      agent,
    });

    if (!response.ok) {
      const problemDetails = (await response.json()) as ProblemDetails;

      throw new TRPCError({
        code: "BAD_REQUEST",
        message: `Error: ${problemDetails.title} - ${problemDetails.detail}`,
      });
    }

    const tokenResponse = (await response.json()) as TokenResponse;

    const cookieStore = await cookies();

    cookieStore.set(ACCESS_TOKEN, tokenResponse.accessToken, {
      httpOnly: true,
      secure: process.env.NODE_ENV === "production",
      path: "/",
      sameSite: "lax",
      maxAge: 60 * 60 * 24 * 1, // 1 day
    });

    cookieStore.set(REFRESH_TOKEN, tokenResponse.refreshToken, {
      httpOnly: true,
      secure: process.env.NODE_ENV === "production",
      path: "/",
      sameSite: "lax",
      maxAge: 60 * 60 * 24 * 7, // 7 days
    });

    return tokenResponse;
  }),
  register: baseProcedure.input(RegisterSchema).mutation(async ({ input }) => {
    const url = `${SERVER_URL}/api/v1/users/register`;

    const response = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(input),
      agent,
    });

    if (!response.ok) {
      const problemDetails = (await response.json()) as ProblemDetails;

      throw new TRPCError({
        code: "BAD_REQUEST",
        message: `Error: ${problemDetails.title} - ${problemDetails.detail}`,
      });
    }

    const tokenResponse = (await response.json()) as TokenResponse;

    const cookieStore = await cookies();

    cookieStore.set(ACCESS_TOKEN, tokenResponse.accessToken, {
      httpOnly: true,
      secure: process.env.NODE_ENV === "production",
      path: "/",
      sameSite: "lax",
      maxAge: 60 * 60 * 24 * 1, // 1 day
    });

    cookieStore.set(REFRESH_TOKEN, tokenResponse.refreshToken, {
      httpOnly: true,
      secure: process.env.NODE_ENV === "production",
      path: "/",
      sameSite: "lax",
      maxAge: 60 * 60 * 24 * 7, // 7 days
    });

    return tokenResponse;
  }),
});
