import axios from "axios";

import { config } from "../utils/config";
import { extractJwt, extractUserInfo } from "../utils/auth";

/**
 * @typedef {Object} Budget
 * @property {string} id
 * @property {string} userId
 * @property {number} buyingPower
 */

/**
 * @returns {Promise<Budget>}
 */
export async function fetchBudget() {
  try {
    const jwt = extractJwt();
    if (!jwt) {
      return null;
    }

    const userInfo = extractUserInfo();

    const response = await axios.get(
      `${config.baseApiUrl}/api/v1/users/${userInfo.id}/budget`,
      {
        headers: {
          Authorization: `Bearer ${jwt}`,
        },
      }
    );

    return response.data;
  } catch (error) {
    console.error(error);
    return null;
  }
}
