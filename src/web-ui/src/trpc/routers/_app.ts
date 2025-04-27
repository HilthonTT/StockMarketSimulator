import { authRouter } from "@/modules/auth/server/procedures";
import { stocksRouter } from "@/modules/stocks/server/procedures";

import { createTRPCRouter } from "@/trpc/init";

export const appRouter = createTRPCRouter({
  auth: authRouter,
  stocks: stocksRouter,
});

// export type definition of API
export type AppRouter = typeof appRouter;
