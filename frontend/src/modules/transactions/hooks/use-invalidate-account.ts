import { useCallback } from "react";
import { useQueryClient } from "@tanstack/react-query";

import { useTRPC } from "@/trpc/client";

/** Refreshes everything a trade can change: cash, positions, history and stats. */
export const useInvalidateAccount = () => {
  const trpc = useTRPC();
  const queryClient = useQueryClient();

  return useCallback(
    () =>
      Promise.all([
        queryClient.invalidateQueries(trpc.budgets.getOne.queryFilter()),
        queryClient.invalidateQueries(trpc.portfolio.getPositions.queryFilter()),
        queryClient.invalidateQueries(trpc.transactions.getMany.pathFilter()),
        queryClient.invalidateQueries(trpc.transactions.getCount.queryFilter()),
        queryClient.invalidateQueries(trpc.users.getPurchasedStockTickers.queryFilter()),
        queryClient.invalidateQueries(trpc.stocks.getTopPerformer.queryFilter()),
      ]),
    [queryClient, trpc]
  );
};
