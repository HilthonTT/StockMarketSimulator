import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

import { normalizeTicker } from "../store";

interface WatchlistState {
  tickers: string[];
  toggle: (ticker: string) => void;
  has: (ticker: string) => boolean;
}

/** Tickers the user follows without owning them. Stored per browser. */
export const useWatchlist = create<WatchlistState>()(
  persist(
    (set, get) => ({
      tickers: [],
      toggle: (rawTicker) => {
        const ticker = normalizeTicker(rawTicker);
        set((state) => ({
          tickers: state.tickers.includes(ticker)
            ? state.tickers.filter((t) => t !== ticker)
            : [...state.tickers, ticker],
        }));
      },
      has: (rawTicker) => get().tickers.includes(normalizeTicker(rawTicker)),
    }),
    {
      name: "sms-watchlist",
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({ tickers: state.tickers }),
      // Rehydrated after mount (see AppShell) so the server and first client render match.
      skipHydration: true,
    }
  )
);
