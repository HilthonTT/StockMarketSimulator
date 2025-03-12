import { config } from "./config.js";
import { ensureAuthenticated } from "./auth.js";

export async function apiFetch(
  endpoint,
  {
    method = "GET",
    body = null,
    requiresAuth = false,
    requiresIdempotencyKey = false,
    throwException = false,
  } = {}
) {
  const url = new URL(`${config.baseApiUrl}${endpoint}`);
  const headers = {
    "Content-Type": "application/json",
  };

  if (requiresAuth) {
    const user = await ensureAuthenticated();
    if (!user) {
      return null;
    }

    headers.Authorization = `Bearer ${user.accessToken}`;
  }

  if (requiresIdempotencyKey) {
    const idempotenceKey = crypto.randomUUID();

    headers["Idempotence-Key"] = idempotenceKey;
  }

  const options = {
    method,
    headers,
    ...(body && { body: JSON.stringify(body) }),
  };

  const response = await fetch(url, options);

  if (!response.ok && throwException) {
    const text = await response.text();
    console.error(text);

    throw new Error("Something went wrong.");
  }

  if (!response.ok && (response.status === 403 || response.status === 401)) {
    window.location.href = "/sign-in/index.html";
    return null;
  }

  if (!response.ok) {
    alert(`Failed to fetch: ${response.status}`);
    return null;
  }

  return method === "GET" ? response.json() : true;
}
