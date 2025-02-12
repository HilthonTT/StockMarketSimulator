import { ensureAuthenticated } from "../utils/auth.js";
import { config } from "../utils/config.js";

export async function fetchTransactions() {
  const user = await ensureAuthenticated();

  const response = await fetch(
    new URL(`${config.baseApiUrl}/api/v1/users/${user.id}/transactions`),
    {
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${user.accessToken}`,
      },
    }
  );

  if (!response.ok) {
    alert("Cannot fetch budget");
    return null;
  }

  const transaction = await response.json();

  return transaction;
}
