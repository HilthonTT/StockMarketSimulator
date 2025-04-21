import axios from "axios";
import { v4 as uuidv4 } from "uuid";

import { config } from "../utils/config.js";
import { extractJwt, extractUserInfo } from "../utils/auth.js";

/**
 * @typedef {Object} BuyTransactionRequest
 * @property {string} userId
 * @property {string} ticker
 * @property {number} quantity
 */

/**
 * @typedef {Object} SellTransactionRequest
 * @property {string} userId
 * @property {string} ticker
 * @property {number} quantity
 */

/**
 * @typedef {Object} TransactionResponse
 * @property {string} id
 * @property {string} userId
 * @property {string} ticker
 * @property {number} limitPrice
 * @property {number} type
 * @property {number} quantity
 */

/**
 * @typedef {Object} PagedTransactionResponse
 * @property {TransactionResponse[]} items
 * @property {number} page
 * @property {number} pageSize
 * @property {number} totalCount
 * @property {boolean} hasNextPage
 * @property {boolean} hasPreviousPage
 */

/**
 *
 * @param {BuyTransactionRequest} request
 */
export async function buyTransaction(request) {
  if (!request) {
    return { error: "Request is null" };
  }

  try {
    const jwt = extractJwt();
    if (!jwt) {
      return { error: "Unauthorized" };
    }

    const url = `${config.baseApiUrl}/api/v1/transactions/buy`;

    await axios.post(url, request, {
      headers: {
        Authorization: `Bearer ${jwt}`,
        "Idempotence-Key": uuidv4(),
      },
    });

    return { success: "Purchase complete!" };
  } catch (error) {
    console.error(error);
    return { error: "Something went wrong!" };
  }
}

/**
 *
 * @param {SellTransactionRequest} request
 */
export async function sellTransaction(request) {
  if (!request) {
    return { error: "Request is null" };
  }

  try {
    const jwt = extractJwt();
    if (!jwt) {
      return { error: "Unauthorized" };
    }

    const url = `${config.baseApiUrl}/api/v1/transactions/sell`;

    await axios.post(url, request, {
      headers: {
        Authorization: `Bearer ${jwt}`,
        "Idempotence-Key": uuidv4(),
      },
    });

    return { success: "Sell complete!" };
  } catch (error) {
    console.error(error);
    return { error: "Something went wrong!" };
  }
}

/**
 * Fetches paginated transactions for the current user.
 *
 * @param {number} [page=1] - The current page number
 * @param {number} [pageSize=10] - Number of transactions per page
 * @param {Object} [filters] - Optional filters for transactions
 * @param {string} [filters.searchTerm] - A search string to filter results
 * @param {Date} [filters.startDate] - Minimum transaction date
 * @param {Date} [filters.endDate] - Maximum transaction date
 * @returns {Promise<PagedTransactionResponse|null>} The paginated transaction response or null on error
 */
export async function getTransactions(page = 1, pageSize = 10, filters = {}) {
  try {
    const jwt = extractJwt();
    if (!jwt) {
      return null;
    }

    const userInfo = extractUserInfo();
    if (!userInfo) {
      return null;
    }

    const url = new URL(
      `${config.baseApiUrl}/api/v1/users/${userInfo.id}/transactions`
    );
    url.searchParams.set("page", page.toString());
    url.searchParams.set("pageSize", pageSize.toString());

    if (filters.searchTerm) {
      url.searchParams.set("searchTerm", filters.searchTerm);
    }

    if (filters.startDate instanceof Date) {
      url.searchParams.set("startDate", filters.startDate.toISOString());
    }

    if (filters.endDate instanceof Date) {
      url.searchParams.set("endDate", filters.endDate.toISOString());
    }

    const response = await axios.get(url, {
      headers: {
        Authorization: `Bearer ${jwt}`,
      },
    });

    return response.data;
  } catch (error) {
    console.error(error);
    return null;
  }
}
