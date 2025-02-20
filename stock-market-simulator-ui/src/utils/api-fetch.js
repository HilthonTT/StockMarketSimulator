import { config } from "./config.js";
import { ensureAuthenticated } from "./auth.js";

export async function apiFetch(
  endpoint,
  { method = "GET", body = null, requiresAuth = false } = {}
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

  const options = {
    method,
    headers,
    ...(body && { body: JSON.stringify(body) }),
  };

  const response = await fetch(url, options);

  if (!response.ok) {
    alert(`Failed to fetch: ${response.status}`);
    return null;
  }

  return method === "GET" ? response.json() : true;
}
