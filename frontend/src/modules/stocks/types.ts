export type StockPriceResponse = {
  ticker: string;
  price: number;
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
