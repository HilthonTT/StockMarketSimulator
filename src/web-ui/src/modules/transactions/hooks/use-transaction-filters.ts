import { parseAsInteger, parseAsString, useQueryStates } from "nuqs";

const params = {
  page: parseAsInteger
    .withOptions({
      clearOnDefault: true,
    })
    .withDefault(1),
  searchTerm: parseAsString
    .withOptions({
      clearOnDefault: true,
    })
    .withDefault(""),
};

export const useTransactionFilters = () => {
  return useQueryStates(params);
};
