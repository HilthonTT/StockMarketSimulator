import { z } from "zod";

import { fetchFromApi } from "@/lib/api";
import { isCryptoTicker } from "@/lib/assets";
import { createTRPCRouter, protectedProcedure } from "@/trpc/init";
import type { PagedList } from "@/types";

import {
  HISTORY_RANGES,
  type MarketNewsItem,
  type MarketOverviewResponse,
  type PriceHistoryResponse,
  type StockPriceResponse,
  type StockQuoteResponse,
  type StockSearchResponse,
} from "../types";

const tickerSchema = z
  .string()
  .trim()
  .min(1)
  .max(12)
  .transform((ticker) => ticker.toUpperCase());

/** Fills day statistics the API may omit (e.g. older servers) so the UI can rely on them. */
const normalizeQuote = (quote: StockQuoteResponse): StockQuoteResponse => {
  const previousClose = quote.previousClose || quote.open || quote.price;
  const change = quote.change ?? quote.price - previousClose;

  return {
    ...quote,
    ticker: quote.ticker.toUpperCase(),
    name: quote.name ?? null,
    sector: quote.sector ?? null,
    assetClass: quote.assetClass ?? (isCryptoTicker(quote.ticker) ? "crypto" : "stock"),
    open: quote.open || quote.price,
    previousClose,
    dayHigh: quote.dayHigh || quote.price,
    dayLow: quote.dayLow || quote.price,
    change,
    changePercent: quote.changePercent ?? (previousClose ? change / previousClose : 0),
  };
};

export const stocksRouter = createTRPCRouter({
  /** Latest simulated quote, or `null` when the ticker is unknown. */
  getOne: protectedProcedure
    .input(z.object({ ticker: tickerSchema }))
    .query(async ({ ctx, input }) => {
      const quote = await fetchFromApi<StockQuoteResponse>({
        path: `/api/v1/stocks/${encodeURIComponent(input.ticker)}`,
        accessToken: ctx.accessToken,
      });

      if (!quote) {
        return null;
      }

      return normalizeQuote(quote);
    }),
  /** The built-in simulated universe with its index. */
  getMarket: protectedProcedure.query(async ({ ctx }) => {
    const market = await fetchFromApi<MarketOverviewResponse>({
      path: `/api/v1/stocks/market`,
      accessToken: ctx.accessToken,
    });

    return market
      ? { ...market, quotes: market.quotes.map(normalizeQuote) }
      : null;
  }),
  getHistory: protectedProcedure
    .input(z.object({ ticker: tickerSchema, range: z.enum(HISTORY_RANGES) }))
    .query(async ({ ctx, input }) => {
      const history = await fetchFromApi<PriceHistoryResponse>({
        path: `/api/v1/stocks/${encodeURIComponent(input.ticker)}/history`,
        accessToken: ctx.accessToken,
        queryParams: { range: input.range },
      });

      return history?.points ?? [];
    }),
  getNews: protectedProcedure
    .input(z.object({ limit: z.number().int().min(1).max(50).default(20) }))
    .query(async ({ ctx, input }) => {
      const news = await fetchFromApi<MarketNewsItem[]>({
        path: `/api/v1/stocks/news`,
        accessToken: ctx.accessToken,
        queryParams: { limit: input.limit },
      });

      return (news ?? []).map((item) => ({ ...item, ticker: item.ticker.toUpperCase() }));
    }),
  search: protectedProcedure
    .input(
      z.object({
        searchTerm: z.string().trim().max(50).optional(),
        /** Page number; named `cursor` so the procedure works with infinite queries. */
        cursor: z.number().int().min(1).optional(),
        pageSize: z.number().int().min(1).max(100).default(20),
      })
    )
    .query(async ({ ctx, input }) => {
      const page = input.cursor ?? 1;

      const result = await fetchFromApi<PagedList<StockSearchResponse>>({
        path: `/api/v1/stocks/search`,
        accessToken: ctx.accessToken,
        queryParams: {
          searchTerm: input.searchTerm || undefined,
          page,
          pageSize: input.pageSize,
        },
      });

      return (
        result ?? {
          items: [],
          page,
          pageSize: input.pageSize,
          totalCount: 0,
          hasNextPage: false,
          hasPreviousPage: false,
        }
      );
    }),
  getTopPerformer: protectedProcedure.query(async ({ ctx }) => {
    const { accessToken, user } = ctx;

    const stockPriceResponse = await fetchFromApi<StockPriceResponse>({
      path: `/api/v1/stocks/top-performer`,
      accessToken,
      headers: {
        userId: user.id,
      },
    });

    return stockPriceResponse ?? null;
  }),
});
