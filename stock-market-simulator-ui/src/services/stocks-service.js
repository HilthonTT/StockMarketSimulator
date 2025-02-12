import { config } from "../utils/config.js";

export async function searchStocks(searchTerm) {
  const response = await fetch(
    new URL(`${config.baseApiUrl}/api/v1/stocks/search/${searchTerm}`),
    {
      headers: {
        "Content-Type": "application/json",
      },
    }
  );

  if (!response.ok) {
    alert("Failed to search stocks");
    return;
  }

  const stockMatchResults = await response.json();

  console.log({ stockMatchResults });

  return stockMatchResults;
}
