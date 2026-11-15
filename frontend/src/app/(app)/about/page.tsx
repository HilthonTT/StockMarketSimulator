import type { Metadata } from "next";
import Link from "next/link";
import {
  ActivityIcon,
  BadgeDollarSignIcon,
  GlobeIcon,
  KeyboardIcon,
  MessageSquareIcon,
  NewspaperIcon,
  ShoppingCartIcon,
  TrophyIcon,
} from "lucide-react";

import { formatCurrency } from "@/lib/utils";
import { STARTING_CASH } from "@/constants";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { PageHeader } from "@/components/page-header";

export const metadata: Metadata = { title: "How it works" };

const SECTIONS = [
  {
    icon: BadgeDollarSignIcon,
    title: "Virtual cash",
    body: `Every account starts with ${formatCurrency(STARTING_CASH)}. It's not real money, so experiment freely: the goal is to grow your account value. You can reset your account from your profile at any time.`,
  },
  {
    icon: GlobeIcon,
    title: "A market of stocks and crypto",
    body: "The simulator ships with its own universe of well-known stocks across sectors, tracked by the price-weighted SIM 30 index, plus crypto pairs like ETH-USD that trade 24/7. You can also look up other symbols.",
  },
  {
    icon: ActivityIcon,
    title: "Live, simulated prices",
    body: "Each price moves about once a second using a Geometric Brownian Motion model, the random-walk model behind options pricing, with a shared market factor so stocks tend to move together. Every asset has an open, a previous close and a day range.",
  },
  {
    icon: NewspaperIcon,
    title: "Market news",
    body: "Now and then a headline hits a company or coin and its price jumps or drops. News appears on the dashboard, and you get a notification when it's about something you own or watch.",
  },
  {
    icon: ShoppingCartIcon,
    title: "Market, limit and stop orders",
    body: "Market orders fill right away at the live price. Limit orders wait for a better price (buy at or below, sell at or above your limit). Stop orders trigger on a move: a stop-loss sells when the price falls to your stop. Triggered orders fill at the market price, with no fees and whole units only.",
  },
  {
    icon: TrophyIcon,
    title: "Performance and the leaderboard",
    body: "Your portfolio is valued at live prices: day change against the previous close, unrealized P&L against your average cost, and all-time return against your starting cash. The leaderboard ranks every trader by account value.",
  },
];

const SHORTCUTS = [
  { keys: "Ctrl / ⌘ + K", action: "Search assets and pages" },
  { keys: "/", action: "Search assets and pages" },
  { keys: "Ctrl / ⌘ + B", action: "Collapse or expand the sidebar" },
];

const Page = () => {
  return (
    <>
      <PageHeader
        title="How the simulator works"
        description="Paper-trade stocks and crypto with live, simulated prices and zero risk."
      />

      <div className="grid gap-4 md:grid-cols-2">
        {SECTIONS.map((section) => (
          <Card key={section.title}>
            <CardHeader className="flex flex-row items-start gap-3">
              <div className="rounded-lg bg-primary/15 p-2 text-primary">
                <section.icon className="size-5" aria-hidden="true" />
              </div>
              <div className="flex flex-col gap-1.5">
                <CardTitle>{section.title}</CardTitle>
                <CardDescription className="leading-relaxed">{section.body}</CardDescription>
              </div>
            </CardHeader>
          </Card>
        ))}
      </div>

      <Card>
        <CardHeader className="flex flex-row items-center gap-3">
          <KeyboardIcon className="size-5" aria-hidden="true" />
          <CardTitle>Keyboard shortcuts</CardTitle>
        </CardHeader>
        <CardContent>
          <dl className="grid gap-2 sm:grid-cols-3">
            {SHORTCUTS.map((shortcut) => (
              <div key={shortcut.keys} className="flex flex-col gap-1 rounded-lg border p-3">
                <dt>
                  <kbd className="rounded border bg-muted px-1.5 py-0.5 font-mono text-xs">{shortcut.keys}</kbd>
                </dt>
                <dd className="text-sm text-muted-foreground">{shortcut.action}</dd>
              </div>
            ))}
          </dl>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Questions or feedback?</CardTitle>
          <CardDescription>
            Found a bug or have an idea? Open an issue on GitHub. The project is open source.
          </CardDescription>
        </CardHeader>
        <CardContent className="flex flex-wrap gap-2">
          <Button asChild>
            <a href="https://github.com/HilthonTT/StockMarketSimulator/issues" target="_blank" rel="noopener noreferrer">
              <MessageSquareIcon /> Open an issue
            </a>
          </Button>
          <Button asChild variant="outline">
            <Link href="/markets">Start trading</Link>
          </Button>
        </CardContent>
      </Card>
    </>
  );
};

export default Page;
