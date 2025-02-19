import { apiFetch } from "../utils/api-fetch.js";
import { ensureAuthenticated } from "../utils/auth.js";

export async function fetchTransactions() {
  const user = await ensureAuthenticated();

  if (!user) {
    return [];
  }

  return (
    (await apiFetch(`/api/v1/users/${user.id}/transactions`, {
      requiresAuth: true,
    })) || []
  );
}

export async function buyTransaction(ticker, quantity) {
  return await apiFetch(`/api/v1/transactions/buy`, {
    method: "POST",
    body: { ticker, quantity },
    requiresAuth: true,
  });
}

export async function sellTransaction(ticker, quantity) {
  return await apiFetch(`/api/v1/transactions/sell`, {
    method: "POST",
    body: { ticker, quantity },
    requiresAuth: true,
  });
}
