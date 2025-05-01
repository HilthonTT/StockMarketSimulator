"use server";

import { cookies } from "next/headers";
import { jwtDecode } from "jwt-decode";

import { TokenResponse } from "@/modules/auth/types";

import { ACCESS_TOKEN, REFRESH_TOKEN } from "@/constants";
import { fetchFromApi } from "@/lib/api";

export async function refreshAccessTokenIfNeeded(): Promise<string | null> {
  const cookieStore = await cookies();

  const accessToken = cookieStore.get(ACCESS_TOKEN)?.value;
  const refreshToken = cookieStore.get(REFRESH_TOKEN)?.value;

  if (!accessToken) {
    return null;
  }

  const decoded = jwtDecode<{ sub: string; exp: number }>(accessToken);

  if (!decoded?.exp || decoded.exp * 1000 > Date.now()) {
    return accessToken; // Token still valid
  }

  if (!refreshToken) {
    return null;
  }

  const tokenResponse = await fetchFromApi<TokenResponse>({
    method: "POST",
    path: "/api/v1/users/refresh-tokens",
    body: { refreshToken },
  });

  if (!tokenResponse) {
    cookieStore.set(ACCESS_TOKEN, "", { maxAge: -1, path: "/" });
    cookieStore.set(REFRESH_TOKEN, "", { maxAge: -1, path: "/" });
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
    maxAge: 60 * 60 * 24 * 1, // 1 day
  });

  cookieStore.set(REFRESH_TOKEN, tokenResponse.refreshToken, {
    httpOnly: true,
    secure: process.env.NODE_ENV === "production",
    path: "/",
    sameSite: "lax",
    maxAge: 60 * 60 * 24 * 7, // 7 days
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
