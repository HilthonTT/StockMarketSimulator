import https from "https";
import fetch from "node-fetch";
import { cookies } from "next/headers";
import { TRPCError } from "@trpc/server";

import { baseProcedure, createTRPCRouter } from "@/trpc/init";
import { ACCESS_TOKEN, REFRESH_TOKEN, SERVER_URL } from "@/constants";
import { ProblemDetails } from "@/types";

import { LoginSchema, RegisterSchema } from "../schemas";
import { TokenResponse } from "../types";

const agent = new https.Agent({
  rejectUnauthorized: false,
});

export const authRouter = createTRPCRouter({
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
