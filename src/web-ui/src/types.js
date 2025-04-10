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
