import { parseAsString, parseAsStringLiteral } from "nuqs";

export const MARKET_TABS = ["stocks", "crypto", "search"] as const;
export type MarketTab = (typeof MARKET_TABS)[number];

export const marketsSearchParams = {
  q: parseAsString.withDefault("").withOptions({ clearOnDefault: true, shallow: true }),
  tab: parseAsStringLiteral(MARKET_TABS).withDefault("stocks").withOptions({ clearOnDefault: true, shallow: true }),
  sector: parseAsString.withDefault("").withOptions({ clearOnDefault: true, shallow: true }),
};
