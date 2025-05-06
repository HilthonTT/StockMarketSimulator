import { authRouter } from "@/modules/auth/server/procedures";
import { budgetsRouter } from "@/modules/budgets/server/procedures";
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
});

// export type definition of API
export type AppRouter = typeof appRouter;
