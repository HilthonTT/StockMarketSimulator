import { create } from "zustand";

import type { AssetClass, MarketNewsItem, StockPriceUpdate, StockQuoteResponse } from "@/modules/stocks/types";
import { isCryptoTicker } from "@/lib/assets";

import { MAX_HISTORY_POINTS, MAX_NEWS_ITEMS } from "./constants";

export type PricePoint = {
  time: number;
  price: number;
};

export type Quote = {
  ticker: string;
  name: string | null;
  sector: string | null;
  assetClass: AssetClass;
  price: number;
  /** Price before the latest tick, used to colour/flash the last move. */
  previousPrice: number;
  /** Day statistics from the simulated session (reset daily by the API). */
  open: number;
  previousClose: number;
  dayHigh: number;
  dayLow: number;
  /** `price - previousClose`, and as a fraction of `previousClose`. */
  change: number;
  changePercent: number;
  updatedAt: number;
  /** Live ticks received in this browser session. */
  history: PricePoint[];
};

export type FeedStatus = "idle" | "connecting" | "live" | "reconnecting" | "offline";

interface FeedState {
  status: FeedStatus;
  quotes: Record<string, Quote>;
  news: MarketNewsItem[];
  setStatus: (status: FeedStatus) => void;
  /** Records a REST quote. Live prices win, but missing metadata (name, sector) is filled in. */
  seed: (quote: StockQuoteResponse) => void;
  /** Applies a live tick. */
  push: (update: StockPriceUpdate) => void;
  /** Adds news items (newest first), ignoring ones already known. */
  addNews: (items: MarketNewsItem[]) => void;
}

export const normalizeTicker = (ticker: string) => ticker.trim().toUpperCase();

const withChange = <T extends { price: number; previousClose: number }>(quote: T) => {
  const change = quote.price - quote.previousClose;

  return {
    ...quote,
    change,
    changePercent: quote.previousClose ? change / quote.previousClose : 0,
  };
};

export const useFeedStore = create<FeedState>((set) => ({
  status: "idle",
  quotes: {},
  news: [],
  setStatus: (status) => set({ status }),
  seed: (incoming) =>
    set((state) => {
      const ticker = normalizeTicker(incoming.ticker);
      const existing = state.quotes[ticker];

      if (existing) {
        if (existing.name === incoming.name && existing.sector === incoming.sector) {
          return state;
        }

        return {
          quotes: {
            ...state.quotes,
            [ticker]: { ...existing, name: incoming.name ?? existing.name, sector: incoming.sector ?? existing.sector },
          },
        };
      }

      const now = Date.now();

      return {
        quotes: {
          ...state.quotes,
          [ticker]: withChange({
            ticker,
            name: incoming.name,
            sector: incoming.sector,
            assetClass: incoming.assetClass,
            price: incoming.price,
            previousPrice: incoming.price,
            open: incoming.open,
            previousClose: incoming.previousClose,
            dayHigh: incoming.dayHigh,
            dayLow: incoming.dayLow,
            updatedAt: now,
            history: [{ time: now, price: incoming.price }],
          }),
        },
      };
    }),
  push: (update) =>
    set((state) => {
      const ticker = normalizeTicker(update.ticker);
      const existing = state.quotes[ticker];
      const now = Date.now();
      const price = update.price;

      const history = existing
        ? [...existing.history, { time: now, price }].slice(-MAX_HISTORY_POINTS)
        : [{ time: now, price }];

      // Older servers only send { ticker, price }: keep what we know.
      const previousClose = update.previousClose || existing?.previousClose || price;

      return {
        quotes: {
          ...state.quotes,
          [ticker]: withChange({
            ticker,
            name: existing?.name ?? null,
            sector: existing?.sector ?? null,
            assetClass: existing?.assetClass ?? (isCryptoTicker(ticker) ? "crypto" : "stock"),
            price,
            previousPrice: existing?.price ?? price,
            open: update.open || existing?.open || price,
            previousClose,
            dayHigh: Math.max(update.dayHigh || price, price),
            dayLow: Math.min(update.dayLow || price, price),
            updatedAt: now,
            history,
          }),
        },
      };
    }),
  addNews: (items) =>
    set((state) => {
      const known = new Set(state.news.map((item) => item.id));
      const fresh = items.filter((item) => !known.has(item.id));

      if (fresh.length === 0) {
        return state;
      }

      const news = [...fresh, ...state.news]
        .sort((a, b) => b.publishedOnUtc.localeCompare(a.publishedOnUtc))
        .slice(0, MAX_NEWS_ITEMS);

      return { news };
    }),
}));
