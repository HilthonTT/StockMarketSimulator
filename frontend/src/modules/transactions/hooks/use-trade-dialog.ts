import { create } from "zustand";

export type TradeSide = "buy" | "sell";

interface TradeDialogState {
  isOpen: boolean;
  ticker: string;
  side: TradeSide;
  open: (ticker: string, side?: TradeSide) => void;
  close: () => void;
}

/** Global "trade this stock" dialog, so any row or card can open an order ticket. */
export const useTradeDialog = create<TradeDialogState>((set) => ({
  isOpen: false,
  ticker: "",
  side: "buy",
  open: (ticker, side = "buy") => set({ isOpen: true, ticker: ticker.toUpperCase(), side }),
  close: () => set({ isOpen: false }),
}));
