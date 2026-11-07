import { TRPCError } from "@trpc/server";
import type { TRPC_ERROR_CODE_KEY } from "@trpc/server/rpc";

import { SERVER_URL } from "@/constants";
import { ProblemDetails } from "@/types";

interface FetchFromApiOptions {
  accessToken?: string;
  path: string;
  method?: "GET" | "POST" | "PUT" | "PATCH" | "DELETE";
  body?: unknown;
  queryParams?: Record<string, string | number | boolean | undefined | Date>;
  headers?: Record<string, string>;
  /**
   * Reads return `null` on 404 so callers can render an empty state. Mutations should
   * set this so a missing resource surfaces as an error instead of a silent success.
   */
  throwOnNotFound?: boolean;
}

const STATUS_TO_TRPC_CODE: Record<number, TRPC_ERROR_CODE_KEY> = {
  400: "BAD_REQUEST",
  404: "NOT_FOUND",
  403: "FORBIDDEN",
  409: "CONFLICT",
  422: "UNPROCESSABLE_CONTENT",
  429: "TOO_MANY_REQUESTS",
};

/**
 * Turns an API problem response into a short, user-facing message.
 * Validation problems carry an `errors` array; everything else uses `detail`/`title`.
 */
function getProblemMessage(problem: Partial<ProblemDetails> | null, status: number) {
  const validationErrors = problem?.errors
    ?.map((error) => error.description)
    .filter(Boolean);

  if (validationErrors && validationErrors.length > 0) {
    return validationErrors.join(" ");
  }

  if (problem?.detail) {
    return problem.detail;
  }

  if (problem?.title) {
    return problem.title;
  }

  if (status === 429) {
    return "You're doing that too often. Please wait a moment and try again.";
  }

  return "Something went wrong. Please try again.";
}

export async function fetchFromApi<T>({
  accessToken,
  path,
  method = "GET",
  body,
  queryParams,
  headers,
  throwOnNotFound = false,
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
  const requestHeaders: Record<string, string> = {
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

  let response: Response;

  try {
    response = await fetch(url, {
      method,
      headers: requestHeaders,
      body: requestBody,
      cache: "no-store",
    });
  } catch (error) {
    throw new TRPCError({
      code: "INTERNAL_SERVER_ERROR",
      message: "The trading server is unreachable. Is the API running?",
      cause: error,
    });
  }

  if (!response.ok) {
    if (
      response.status === 401 ||
      (response.status === 404 && !throwOnNotFound)
    ) {
      return null;
    }

    // Error bodies aren't guaranteed to be JSON (proxies, crashes, rate limiters).
    const problem = (await response
      .json()
      .catch(() => null)) as Partial<ProblemDetails> | null;

    throw new TRPCError({
      code: STATUS_TO_TRPC_CODE[response.status] ?? "INTERNAL_SERVER_ERROR",
      message: getProblemMessage(problem, response.status),
    });
  }

  if (response.status === 204) {
    return null;
  }

  const text = await response.text();

  return text ? (JSON.parse(text) as T) : null;
}
