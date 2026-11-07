import { useQueryStates } from "nuqs";

import { transactionFilterParams } from "../search-params";

export const useTransactionFilters = () =>
  useQueryStates(transactionFilterParams, { clearOnDefault: true });
