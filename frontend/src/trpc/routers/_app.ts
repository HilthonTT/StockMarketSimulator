import { authRouter } from "@/modules/auth/server/procedures";
import { budgetsRouter } from "@/modules/budgets/server/procedures";
import { leaderboardRouter } from "@/modules/leaderboard/server/procedures";
import { ordersRouter } from "@/modules/orders/server/procedures";
import { portfolioRouter } from "@/modules/portfolio/server/procedures";
import { shortenRouter } from "@/modules/shorten/server/procedures";
import { stocksRouter } from "@/modules/stocks/server/procedures";
import { transactionsRouter } from "@/modules/transactions/server/procedures";
import { usersRouter } from "@/modules/users/server/procedures";

import { createTRPCRouter } from "@/trpc/init";

export const appRouter = createTRPCRouter({
  auth: authRouter,
  budgets: budgetsRouter,
  stocks: stocksRouter,
  transactions: transactionsRouter,
  users: usersRouter,
  shorten: shortenRouter,
  portfolio: portfolioRouter,
  orders: ordersRouter,
  leaderboard: leaderboardRouter,
});

// export type definition of API
export type AppRouter = typeof appRouter;
