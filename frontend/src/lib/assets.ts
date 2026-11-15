/** Crypto pairs are quoted against the dollar, e.g. `ETH-USD`. */
export const isCryptoTicker = (ticker: string) => /-USD$/i.test(ticker.trim());

/** `ETH-USD` → `ETH`; stocks are returned unchanged. */
export const displaySymbol = (ticker: string) => ticker.replace(/-USD$/i, "");

/** Path to an asset's page; tickers like `ETH-USD` are URL-safe but are encoded anyway. */
export const assetHref = (ticker: string) => `/stocks/${encodeURIComponent(ticker.toUpperCase())}`;

const HUES = [268, 200, 160, 120, 30, 340, 300, 230, 80, 10, 185, 250];

/** Stable per-ticker hue so monograms are recognisable at a glance. */
export const tickerHue = (ticker: string) => {
  let hash = 0;
  for (const char of ticker) {
    hash = (hash * 31 + char.charCodeAt(0)) >>> 0;
  }
  return HUES[hash % HUES.length];
};
