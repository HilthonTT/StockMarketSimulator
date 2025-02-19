import { apiFetch } from "../utils/api-fetch.js";

export async function searchStocks(searchTerm) {
  if (!searchTerm) {
    return [];
  }

  return await apiFetch(`/api/v1/stocks/search/${searchTerm}`);
}
