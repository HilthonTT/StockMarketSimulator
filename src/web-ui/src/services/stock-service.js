import axios from "axios";

import { config } from "../utils/config.js";
import { PAGE_SIZE } from "../constants/index.js";
import { extractJwt } from "../utils/auth.js";

/**
 * Searches for stocks based on a ticker symbol.
 *
 * @param {string} ticker - The ticker symbol to search for.
 * @param {number} page - The page number for pagination.
 * @returns {Promise<PagedListStockSearchResponse|null>} - The paged search result or null if no ticker is provided.
 */
export async function searchStocks(ticker, page) {
  if (!ticker) {
    return null;
  }

  const jwt = extractJwt();
  if (!jwt) {
    return null;
  }

  const url = `${config.baseApiUrl}/api/v1/stocks/search`;
  const response = await axios.get(url, {
    params: {
      searchTerm: ticker,
      page,
      pageSize: PAGE_SIZE,
    },
    headers: {
      Authorization: `Bearer ${jwt}`,
    },
  });

  const pagedStockSearch = response.data;

  return pagedStockSearch;
}
