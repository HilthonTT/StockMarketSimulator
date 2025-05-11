import { createLoader, parseAsIsoDate, parseAsString } from "nuqs/server";

const params = {
  searchTerm: parseAsString
    .withOptions({
      clearOnDefault: true,
    })
    .withDefault(""),
  startDate: parseAsIsoDate
    .withOptions({ clearOnDefault: true })
    .withDefault(new Date(0)),
  endDate: parseAsIsoDate
    .withOptions({ clearOnDefault: true })
    .withDefault(new Date(0)),
};

export const loadTransactionFilters = createLoader(params);
