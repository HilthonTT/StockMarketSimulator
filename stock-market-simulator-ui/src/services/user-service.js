import { ensureAuthenticated } from "../utils/auth.js";
import { apiFetch } from "../utils/api-fetch.js";

export async function revokeRefreshTokens() {
  const user = await ensureAuthenticated();

  if (!user) {
    return null;
  }

  return await apiFetch(`/api/v1/users/${user?.id}/refresh-tokens`, {
    method: "DELETE",
    requiresAuth: true,
    throwException: true,
  });
}
