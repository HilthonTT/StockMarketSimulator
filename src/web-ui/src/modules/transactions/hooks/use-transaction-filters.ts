import { parseAsString, useQueryStates } from "nuqs";

const params = {
  searchTerm: parseAsString
    .withOptions({
      clearOnDefault: true,
    })
    .withDefault(""),
};

export const useTransactionFilters = () => {
  return useQueryStates(params);
};
