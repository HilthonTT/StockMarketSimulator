/** Price-only payload (top performer). */
export type StockPriceResponse = {
  ticker: string;
  price: number;
};

export type AssetClass = "stock" | "crypto";

/** Full quote with day statistics. `changePercent` is a fraction (0.0123 = 1.23%). */
export type StockQuoteResponse = {
  ticker: string;
  name: string | null;
  sector: string | null;
  assetClass: AssetClass;
  price: number;
  open: number;
  previousClose: number;
  dayHigh: number;
  dayLow: number;
  change: number;
  changePercent: number;
  updatedOnUtc: string;
};

/** Live tick pushed over SignalR (`ReceiveStockPriceUpdate`). */
export type StockPriceUpdate = {
  ticker: string;
  price: number;
  open: number;
  previousClose: number;
  dayHigh: number;
  dayLow: number;
  timestamp: string;
};

export type MarketIndex = {
  name: string;
  value: number;
  change: number;
  changePercent: number;
};

export type MarketOverviewResponse = {
  index: MarketIndex;
  quotes: StockQuoteResponse[];
  /** Last 24h of closes in 30-minute steps, per built-in ticker. */
  sparklines?: Record<string, number[]>;
};

export const HISTORY_RANGES = ["1D", "1W", "1M", "3M"] as const;
export type HistoryRange = (typeof HISTORY_RANGES)[number];

export type PriceBar = {
  time: string;
  open: number;
  high: number;
  low: number;
  close: number;
};

export type PriceHistoryResponse = {
  ticker: string;
  range: HistoryRange;
  points: PriceBar[];
};

export type MarketNewsItem = {
  id: string;
  ticker: string;
  headline: string;
  impactPercent: number;
  sentiment: "positive" | "negative";
  publishedOnUtc: string;
};

export type PurchasedStockTickersResponse = {
  tickers: string[];
};

export type StockSearchResponse = {
  ticker: string;
  name: string;
  type: string;
  region: string;
  marketOpen: string;
  timezone: string;
  currency: string;
};
