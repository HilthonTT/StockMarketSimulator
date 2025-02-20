import { apiFetch } from "../utils/api-fetch.js";

export async function searchStocks(searchTerm) {
  if (!searchTerm) {
    return [];
  }

  return await apiFetch(`/api/v1/stocks/search/${searchTerm}`);
}

export async function fetchPrice(ticker) {
  if (!ticker) {
    return null;
  }

  return await apiFetch(`/api/v1/stocks/${ticker}`);
}
