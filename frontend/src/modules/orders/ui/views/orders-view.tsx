"use client";

import Link from "next/link";
import { useSuspenseQuery } from "@tanstack/react-query";
import { ClipboardListIcon } from "lucide-react";

import { useTRPC } from "@/trpc/client";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { PageHeader } from "@/components/page-header";
import { QueryBoundary } from "@/components/query-boundary";
import { EmptyState } from "@/components/empty-state";

import { OrderList } from "../components/order-list";

export const OrdersView = () => (
  <>
    <PageHeader
      title="Orders"
      description="Limit and stop orders wait here until the live price reaches your trigger, then fill at the market price."
    />
    <QueryBoundary fallback={<Skeleton className="h-[420px] w-full rounded-xl" />} errorTitle="Couldn't load your orders">
      <Orders />
    </QueryBoundary>
  </>
);

const Orders = () => {
  const trpc = useTRPC();
  // Polls while orders are pending so fills appear without a refresh.
  const { data: orders } = useSuspenseQuery(
    trpc.orders.getMany.queryOptions(
      { status: "all" },
      { refetchInterval: (query) => (query.state.data?.some((order) => order.status === "pending") ? 5_000 : false) }
    )
  );

  const open = orders.filter((order) => order.status === "pending");
  const past = orders.filter((order) => order.status !== "pending");

  const empty = (title: string, description: string) => (
    <EmptyState
      icon={ClipboardListIcon}
      title={title}
      description={description}
      action={
        <Button asChild variant="outline">
          <Link href="/markets">Browse markets</Link>
        </Button>
      }
      className="py-12"
    />
  );

  return (
    <Tabs defaultValue="open" className="gap-4">
      <TabsList>
        <TabsTrigger value="open">
          Open <span className="num ml-1 text-muted-foreground">{open.length}</span>
        </TabsTrigger>
        <TabsTrigger value="past">
          Filled &amp; closed <span className="num ml-1 text-muted-foreground">{past.length}</span>
        </TabsTrigger>
      </TabsList>
      <TabsContent value="open">
        <Card className="gap-0 overflow-hidden py-0">
          {open.length > 0
            ? <OrderList orders={open} />
            : empty("No open orders", "Pick a stock or coin and choose Limit or Stop in the order ticket to set a trigger price.")}
        </Card>
      </TabsContent>
      <TabsContent value="past">
        <Card className="gap-0 overflow-hidden py-0">
          {past.length > 0
            ? <OrderList orders={past} />
            : empty("Nothing here yet", "Filled, cancelled and rejected orders are kept here.")}
        </Card>
      </TabsContent>
    </Tabs>
  );
};
