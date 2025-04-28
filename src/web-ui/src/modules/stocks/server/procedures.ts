import { createTRPCRouter, protectedProcedure } from "@/trpc/init";

export const stocksRouter = createTRPCRouter({
  getMany: protectedProcedure.query(() => {
    const data = [
      {
        ticker: "TSLA",
      },
    ];

    return data;
  }),
});
