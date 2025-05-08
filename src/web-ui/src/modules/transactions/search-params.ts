import { createLoader, parseAsInteger, parseAsString } from "nuqs/server";

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

export const loadTransactionFilters = createLoader(params);
