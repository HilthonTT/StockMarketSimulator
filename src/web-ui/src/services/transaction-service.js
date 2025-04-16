import axios from "axios";

import { config } from "../utils/config.js";
import { extractJwt } from "../utils/auth.js";

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
      },
    });

    return { success: "Sell complete!" };
  } catch (error) {
    console.error(error);
    return { error: "Something went wrong!" };
  }
}
