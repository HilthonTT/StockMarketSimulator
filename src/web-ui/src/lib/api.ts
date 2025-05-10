import { SERVER_URL } from "@/constants";
import { ProblemDetails } from "@/types";
import { TRPCError } from "@trpc/server";

interface FetchFromApiOptions {
  accessToken?: string;
  path: string;
  method?: "GET" | "POST" | "PUT" | "PATCH" | "DELETE";
  body?: unknown;
  queryParams?: Record<string, string | number | boolean | undefined | Date>;
  headers?: Record<string, string>;
}

export async function fetchFromApi<T>({
  accessToken,
  path,
  method = "GET",
  body,
  queryParams,
  headers,
}: FetchFromApiOptions): Promise<T | null> {
  let url = `${SERVER_URL}${path}`;

  if (queryParams) {
    const params = new URLSearchParams();
    for (const [key, value] of Object.entries(queryParams)) {
      if (value !== undefined && value !== null) {
        const formattedValue =
          value instanceof Date ? value.toISOString() : value.toString();

        params.append(key, formattedValue);
      }
    }
    url += `?${params.toString()}`;
  }

  let requestBody: BodyInit | undefined;
  const requestHeaders: HeadersInit = {
    ...(accessToken && { Authorization: `Bearer ${accessToken}` }),
    ...headers,
  };

  if (body instanceof File) {
    const formData = new FormData();
    formData.append("file", body);
    requestBody = formData;
  } else if (body) {
    requestHeaders["Content-Type"] = "application/json";
    requestBody = JSON.stringify(body);
  }

  const response = await fetch(url, {
    method,
    headers: requestHeaders,
    body: requestBody,
  });

  if (!response.ok) {
    if (response.status === 401 || response.status === 404) {
      return null;
    }

    const problemDetails = (await response.json()) as ProblemDetails;

    throw new TRPCError({
      code: "BAD_REQUEST",
      message: `Error: ${problemDetails.title} - ${problemDetails.detail}`,
    });
  }

  if (response.status !== 204) {
    return response.json() as Promise<T>;
  }

  return null;
}
