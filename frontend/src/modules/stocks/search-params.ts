import { parseAsString } from "nuqs";

export const marketsSearchParams = {
  q: parseAsString.withDefault("").withOptions({ clearOnDefault: true, shallow: true }),
};
