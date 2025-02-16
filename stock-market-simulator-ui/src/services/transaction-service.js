import { ensureAuthenticated } from "../utils/auth.js";
import { config } from "../utils/config.js";

export async function fetchTransactions() {
  const user = await ensureAuthenticated();
  if (!user) {
    return [];
  }

  const url = new URL(
    `${config.baseApiUrl}/api/v1/users/${user.id}/transactions`
  );

  const response = await fetch(url, {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${user.accessToken}`,
    },
  });

  if (!response.ok) {
    alert("Cannot fetch budget");
    return null;
  }

  const transactions = await response.json();

  return transactions;
}

export async function buyTransaction(ticker, quantity) {
  const user = await ensureAuthenticated();

  if (!user) {
    return;
  }

  const url = new URL(`${config.baseApiUrl}/api/v1/transactions/buy`);

  const response = await fetch(url, {
    method: "POST",
    body: JSON.stringify({
      ticker,
      quantity,
    }),
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${user.accessToken}`,
    },
  });

  if (!response.ok) {
    alert("Failed to buy stocks");
    return;
  }
}

export async function sellTransaction(ticker, quantity) {
  const user = await ensureAuthenticated();

  if (!user) {
    return;
  }

  const url = new URL(`${config.baseApiUrl}/api/v1/transactions/sell`);

  const response = await fetch(url, {
    method: "POST",
    body: JSON.stringify({
      ticker,
      quantity,
    }),
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${user.accessToken}`,
    },
  });

  if (!response.ok) {
    alert("Failed to sell stocks");
    return;
  }
}
