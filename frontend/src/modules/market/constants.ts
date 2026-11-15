export const SIGNALR_HUB_PATH = "/stocks-feed";
export const SIGNALR_JOIN_GROUP = "JoinGroup";
export const SIGNALR_LEAVE_GROUP = "LeaveGroup";
export const SIGNALR_STOCK_UPDATE_EVENT = "ReceiveStockPriceUpdate";
export const SIGNALR_MARKET_NEWS_EVENT = "ReceiveMarketNews";

/** Ticks kept per ticker for live charts (the feed ticks about once a second). */
export const MAX_HISTORY_POINTS = 600;

/** News items kept in memory for the news feed. */
export const MAX_NEWS_ITEMS = 50;
