import { NextResponse } from "next/server";

import { TokenResponse } from "@/modules/auth/types";
import { LoginSchema } from "@/modules/auth/schemas";

import { fetchFromApi } from "@/lib/api";
import { setAuthCookies } from "@/lib/auth";

export async function POST(req: Request) {
  const body = await req.json();

  const result = await LoginSchema.safeParseAsync(body);
  if (!result.success) {
    return NextResponse.json(
      {
        error: "Validation failed",
        issues: result.error.format(), // or result.error.issues for raw errors
      },
      { status: 400 }
    );
  }

  const tokenResponse = await fetchFromApi<TokenResponse>({
    path: "/api/v1/users/login",
    method: "POST",
    body: body,
  });

  if (tokenResponse) {
    await setAuthCookies(tokenResponse);
  }

  return NextResponse.json(tokenResponse);
}
