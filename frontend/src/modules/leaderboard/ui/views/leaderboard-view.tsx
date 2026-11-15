"use client";

import { useQuery, useSuspenseQuery } from "@tanstack/react-query";
import { CrownIcon, MedalIcon, TrophyIcon } from "lucide-react";

import { useTRPC } from "@/trpc/client";
import { cn, formatCurrency, formatNumber } from "@/lib/utils";
import { STARTING_CASH } from "@/constants";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { PageHeader } from "@/components/page-header";
import { QueryBoundary } from "@/components/query-boundary";
import { EmptyState } from "@/components/empty-state";
import { PercentChange } from "@/modules/market/ui/components/price-change";

import type { LeaderboardEntry } from "../../types";
import { LEADERBOARD_SIZE } from "../../constants";


export const LeaderboardView = () => (
  <>
    <PageHeader
      title="Leaderboard"
      description={`Every trader starts with ${formatCurrency(STARTING_CASH)}. Ranked by account value: cash plus holdings at live prices.`}
    />
    <QueryBoundary fallback={<Skeleton className="h-[520px] w-full rounded-xl" />} errorTitle="Couldn't load the leaderboard">
      <Leaderboard />
    </QueryBoundary>
  </>
);

const PODIUM_STYLES = [
  { icon: CrownIcon, ring: "ring-amber-400/60", text: "text-amber-400", label: "1st" },
  { icon: MedalIcon, ring: "ring-slate-300/60", text: "text-slate-300", label: "2nd" },
  { icon: MedalIcon, ring: "ring-orange-400/60", text: "text-orange-400", label: "3rd" },
];

const Leaderboard = () => {
  const trpc = useTRPC();
  const { data } = useSuspenseQuery(
    trpc.leaderboard.get.queryOptions({ limit: LEADERBOARD_SIZE }, { refetchInterval: 30_000 })
  );
  const { data: me } = useQuery(trpc.users.getCurrent.queryOptions());
  const myId = data.currentUser?.userId ?? me?.id;

  if (data.entries.length === 0) {
    return (
      <EmptyState
        icon={TrophyIcon}
        title="No traders ranked yet"
        description="Make a trade and you'll show up here."
      />
    );
  }

  const podium = data.entries.slice(0, 3);
  const isMeOnBoard = data.entries.some((entry) => entry.userId === myId);

  return (
    <>
      <div className="grid gap-4 md:grid-cols-3">
        {podium.map((entry, index) => {
          const style = PODIUM_STYLES[index];

          return (
            <Card
              key={entry.userId}
              className={cn("gap-3 ring-1 ring-inset", style.ring, entry.userId === myId && "bg-primary/5")}
            >
              <CardHeader>
                <CardDescription className={cn("flex items-center gap-1.5 font-semibold", style.text)}>
                  <style.icon className="size-4" aria-hidden="true" />
                  {style.label}
                </CardDescription>
                <CardTitle className="flex items-center gap-2 truncate text-lg">
                  {entry.username}
                  {entry.userId === myId && <Badge>You</Badge>}
                </CardTitle>
              </CardHeader>
              <CardContent>
                <p className="num text-2xl font-bold">{formatCurrency(entry.accountValue)}</p>
                <PercentChange ratio={entry.returnPercent} className="num text-sm" />
              </CardContent>
            </Card>
          );
        })}
      </div>

      {data.currentUser && !isMeOnBoard && <MyRank entry={data.currentUser} />}

      <Card className="gap-0 overflow-hidden py-0">
        <Table>
          <TableHeader>
            <TableRow className="bg-muted/50 hover:bg-muted/50">
              <TableHead className="w-16 pl-4">Rank</TableHead>
              <TableHead>Trader</TableHead>
              <TableHead className="text-right">Account value</TableHead>
              <TableHead className="text-right">Return</TableHead>
              <TableHead className="hidden text-right md:table-cell">Cash</TableHead>
              <TableHead className="hidden text-right md:table-cell">Holdings</TableHead>
              <TableHead className="hidden pr-4 text-right sm:table-cell">Positions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.entries.map((entry) => (
              <Row key={entry.userId} entry={entry} isMe={entry.userId === myId} />
            ))}
          </TableBody>
        </Table>
      </Card>
    </>
  );
};

const Row = ({ entry, isMe }: { entry: LeaderboardEntry; isMe: boolean }) => (
  <TableRow className={cn(isMe && "bg-primary/10 hover:bg-primary/15")} aria-current={isMe ? "true" : undefined}>
    <TableCell className="num pl-4 font-semibold">#{entry.rank}</TableCell>
    <TableCell className="font-medium">
      <span className="flex items-center gap-2">
        {entry.username}
        {isMe && <Badge>You</Badge>}
      </span>
    </TableCell>
    <TableCell className="num text-right font-semibold">{formatCurrency(entry.accountValue)}</TableCell>
    <TableCell className="text-right">
      <PercentChange ratio={entry.returnPercent} className="num" />
    </TableCell>
    <TableCell className="num hidden text-right text-muted-foreground md:table-cell">{formatCurrency(entry.cash)}</TableCell>
    <TableCell className="num hidden text-right text-muted-foreground md:table-cell">
      {formatCurrency(entry.holdingsValue)}
    </TableCell>
    <TableCell className="num hidden pr-4 text-right sm:table-cell">{formatNumber(entry.positions)}</TableCell>
  </TableRow>
);

const MyRank = ({ entry }: { entry: LeaderboardEntry }) => (
  <Card className="flex-row items-center justify-between gap-4 bg-primary/5 px-6">
    <div>
      <p className="text-sm text-muted-foreground">Your rank</p>
      <p className="num text-2xl font-bold">#{formatNumber(entry.rank)}</p>
    </div>
    <div className="text-right">
      <p className="num text-lg font-semibold">{formatCurrency(entry.accountValue)}</p>
      <PercentChange ratio={entry.returnPercent} className="num text-sm" />
    </div>
  </Card>
);
