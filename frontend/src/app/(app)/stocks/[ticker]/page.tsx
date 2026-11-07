import type { Metadata } from "next";

import { StockView } from "@/modules/stocks/ui/views/stock-view";
import { STOCK_TRADES_PAGE_SIZE } from "@/modules/stocks/constants";

import { getQueryClient, HydrateClient, trpc } from "@/trpc/server";

interface PageProps {
  params: Promise<{ ticker: string }>;
}

const normalize = (ticker: string) => decodeURIComponent(ticker).trim().toUpperCase().slice(0, 10);

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { ticker } = await params;

  return { title: normalize(ticker) };
}

const Page = async ({ params }: PageProps) => {
  const ticker = normalize((await params).ticker);

  const queryClient = getQueryClient();
  void queryClient.prefetchQuery(trpc.stocks.getOne.queryOptions({ ticker }));
  void queryClient.prefetchQuery(
    trpc.transactions.getMany.queryOptions({ searchTerm: ticker, pageSize: STOCK_TRADES_PAGE_SIZE })
  );

  return (
    <HydrateClient>
      {/* Keyed so navigating between tickers resets all local state. */}
      <StockView key={ticker} ticker={ticker} />
    </HydrateClient>
  );
};

export default Page;
