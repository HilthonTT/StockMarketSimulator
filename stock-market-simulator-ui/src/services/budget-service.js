import { ensureAuthenticated } from "../utils/auth.js";
import { config } from "../utils/config.js";

export async function fetchBudget() {
  const user = await ensureAuthenticated();

  const response = await fetch(
    new URL(`${config.baseApiUrl}/api/v1/users/${user.id}/budgets`),
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

  const budget = await response.json();

  return budget;
}
