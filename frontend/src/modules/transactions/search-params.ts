import { createLoader, parseAsIsoDateTime, parseAsString } from "nuqs/server";

export const HISTORY_PAGE_SIZE = 25;

/**
 * History filters. Dates are full timestamps (start/end of the picked local days) so the
 * server prefetch and the client query use identical inputs regardless of time zone.
 */
export const transactionFilterParams = {
  search: parseAsString.withDefault(""),
  from: parseAsIsoDateTime,
  to: parseAsIsoDateTime,
};

export const loadTransactionFilters = createLoader(transactionFilterParams);

export type TransactionFilters = {
  search: string;
  from: Date | null;
  to: Date | null;
};

export const toTransactionQueryInput = (filters: TransactionFilters) => ({
  pageSize: HISTORY_PAGE_SIZE,
  searchTerm: filters.search.trim() || undefined,
  startDate: filters.from ?? undefined,
  endDate: filters.to ?? undefined,
});
