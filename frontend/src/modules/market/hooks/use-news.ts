"use client";

import { useEffect } from "react";
import { useQuery } from "@tanstack/react-query";

import { useTRPC } from "@/trpc/client";

import { useFeedStore } from "../store";

/** Loads recent headlines once; new ones arrive over SignalR (see StockFeedProvider). */
export function useNewsSeed() {
  const trpc = useTRPC();
  const addNews = useFeedStore((state) => state.addNews);
  const { data } = useQuery(trpc.stocks.getNews.queryOptions({ limit: 20 }, { staleTime: 5 * 60_000 }));

  useEffect(() => {
    if (data) {
      addNews(data);
    }
  }, [data, addNews]);
}
