import axios from "axios";

import { config } from "../utils/config.js";
import { PAGE_SIZE } from "../constants/index.js";
import { extractJwt } from "../utils/auth.js";

/**
 * @typedef {Object} PagedList<T>
 * @property {T[]} items
 * @property {number} totalCount
 * @property {number} pageIndex
 * @property {number} pageSize
 */

/**
 * @typedef {Object} StockSearchResponse
 * @property {string} ticker
 * @property {string} name
 * @property {string} type
 * @property {string} region
 * @property {string} marketOpen
 * @property {string} timezone
 * @property {string} currency
 */

/**
 * @typedef {Object} PagedListStockSearchResponse
 * @property {StockSearchResponse[]} items
 * @property {number} page
 * @property {number} pageSize
 * @property {number} totalCount
 * @property {boolean} hasNextPage
 * @property {boolean} hasPreviousPage
 */

/**
 * @typedef {Object} StockPriceResponse
 * @property {string} ticker
 * @property {number} price
 */

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

  try {
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
  } catch (error) {
    console.error(error);
    return null;
  }
}

/**
 * Fetches stock based on a ticker symbol.
 *
 * @param {string} ticker - The ticker symbol.
 * @returns {Promise<StockPriceResponse|null>} - The stock price response.
 */
export async function fetchStockPrice(ticker) {
  if (!ticker) {
    return null;
  }

  try {
    const jwt = extractJwt();
    if (!jwt) {
      return null;
    }

    const url = `${config.baseApiUrl}/api/v1/stocks/${ticker}`;
    const response = await axios.get(url, {
      headers: {
        Authorization: `Bearer ${jwt}`,
      },
    });

    const stockPriceResponse = response.data;

    return stockPriceResponse;
  } catch (error) {
    console.error(error);
    return null;
  }
}
