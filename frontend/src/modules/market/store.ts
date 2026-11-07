import { create } from "zustand";

import { MAX_HISTORY_POINTS } from "./constants";

export type PricePoint = {
  time: number;
  price: number;
};

export type Quote = {
  ticker: string;
  price: number;
  /** Price before the latest tick, used to colour/flash the last move. */
  previousPrice: number;
  /** First price seen in this browser session; the baseline for "session change". */
  sessionOpen: number;
  updatedAt: number;
  history: PricePoint[];
};

export type FeedStatus = "idle" | "connecting" | "live" | "reconnecting" | "offline";

interface FeedState {
  status: FeedStatus;
  quotes: Record<string, Quote>;
  setStatus: (status: FeedStatus) => void;
  /** Records a REST quote only if the ticker has no live data yet. */
  seed: (ticker: string, price: number) => void;
  /** Applies a live tick. */
  push: (ticker: string, price: number) => void;
}

export const normalizeTicker = (ticker: string) => ticker.trim().toUpperCase();

export const useFeedStore = create<FeedState>((set) => ({
  status: "idle",
  quotes: {},
  setStatus: (status) => set({ status }),
  seed: (rawTicker, price) =>
    set((state) => {
      const ticker = normalizeTicker(rawTicker);

      if (state.quotes[ticker]) {
        return state;
      }

      const now = Date.now();

      return {
        quotes: {
          ...state.quotes,
          [ticker]: {
            ticker,
            price,
            previousPrice: price,
            sessionOpen: price,
            updatedAt: now,
            history: [{ time: now, price }],
          },
        },
      };
    }),
  push: (rawTicker, price) =>
    set((state) => {
      const ticker = normalizeTicker(rawTicker);
      const existing = state.quotes[ticker];
      const now = Date.now();

      const history = existing
        ? [...existing.history, { time: now, price }].slice(-MAX_HISTORY_POINTS)
        : [{ time: now, price }];

      return {
        quotes: {
          ...state.quotes,
          [ticker]: {
            ticker,
            price,
            previousPrice: existing?.price ?? price,
            sessionOpen: existing?.sessionOpen ?? price,
            updatedAt: now,
            history,
          },
        },
      };
    }),
}));
