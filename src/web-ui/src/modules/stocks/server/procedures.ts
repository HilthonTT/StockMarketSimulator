import { createTRPCRouter, protectedProcedure } from "@/trpc/init";

export const stocksRouter = createTRPCRouter({
  getMany: protectedProcedure.query(({ ctx }) => {
    const { userId } = ctx;

    console.log({ userId });

    const data = [
      {
        ticker: "TSLA",
      },
    ];

    return data;
  }),
});
