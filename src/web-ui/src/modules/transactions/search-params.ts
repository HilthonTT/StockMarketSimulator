import { createLoader, parseAsString } from "nuqs/server";

const params = {
  searchTerm: parseAsString
    .withOptions({
      clearOnDefault: true,
    })
    .withDefault(""),
};

export const loadTransactionFilters = createLoader(params);
