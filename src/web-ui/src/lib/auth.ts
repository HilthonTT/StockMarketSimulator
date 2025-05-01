"use server";

import { cookies } from "next/headers";
import { jwtDecode } from "jwt-decode";

import { TokenResponse } from "@/modules/auth/types";

import { ACCESS_TOKEN, REFRESH_TOKEN } from "@/constants";
import { fetchFromApi } from "@/lib/api";

const EXPIRATION_TIME = 60 * 60 * 24 * 10; // 10 DAYS

export async function refreshAccessTokenIfNeeded(): Promise<string | null> {
  const { accessToken, refreshToken } = await getDecodedTokenFromCookies();

  if (!accessToken) {
    return null;
  }

  if (isTokenExpired(accessToken)) {
    return accessToken; // Token still valid
  }

  if (!refreshToken) {
    return null;
  }

  const tokenResponse = await fetchFromApi<TokenResponse>({
    method: "POST",
    path: "/api/v1/users/refresh-tokens",
    body: { refreshToken: refreshToken ?? "" },
  });

  if (!tokenResponse) {
    await clearAuthCookies();
    return null;
  }

  await setAuthCookies(tokenResponse);

  return tokenResponse.accessToken;
}

export async function setAuthCookies(
  tokenResponse: TokenResponse
): Promise<void> {
  const cookieStore = await cookies();

  cookieStore.set(ACCESS_TOKEN, tokenResponse.accessToken, {
    httpOnly: true,
    secure: process.env.NODE_ENV === "production",
    path: "/",
    sameSite: "lax",
    maxAge: EXPIRATION_TIME,
  });

  cookieStore.set(REFRESH_TOKEN, tokenResponse.refreshToken, {
    httpOnly: true,
    secure: process.env.NODE_ENV === "production",
    path: "/",
    sameSite: "lax",
    maxAge: EXPIRATION_TIME,
  });
}

export async function clearAuthCookies(): Promise<void> {
  const cookieStore = await cookies();

  cookieStore.delete(ACCESS_TOKEN);
  cookieStore.delete(REFRESH_TOKEN);
}

export async function getDecodedTokenFromCookies() {
  const cookieStore = await cookies();

  const accessToken = cookieStore.get(ACCESS_TOKEN)?.value;
  const refreshToken = cookieStore.get(REFRESH_TOKEN)?.value;

  if (!accessToken || !refreshToken) {
    return { accessToken: "", refreshToken: "", userId: "" };
  }

  const decoded = jwtDecode<{ sub: string; exp: number }>(accessToken);

  return { accessToken, refreshToken, userId: decoded.sub };
}

function isTokenExpired(accessToken: string): boolean {
  const decoded = jwtDecode<{ sub: string; exp: number }>(accessToken);

  const isExpired = decoded.exp * 1000 > Date.now();

  return isExpired;
}
