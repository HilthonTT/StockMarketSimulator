import { NextResponse } from "next/server";

import { refreshAccessTokenIfNeeded } from "@/lib/auth";

export async function POST() {
  console.log("Hit post");

  const accessToken = await refreshAccessTokenIfNeeded();

  console.log({ accessToken });

  return NextResponse.json(accessToken);
}
