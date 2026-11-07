import type { Metadata } from "next";
import Link from "next/link";
import {
  ActivityIcon,
  BadgeDollarSignIcon,
  MessageSquareIcon,
  KeyboardIcon,
  LineChartIcon,
  ShoppingCartIcon,
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
    body: `Every account starts with ${formatCurrency(STARTING_CASH)}. It's not real money, so experiment freely: the goal is to grow your account value.`,
  },
  {
    icon: ActivityIcon,
    title: "Live, simulated prices",
    body: "Starting prices come from real market quotes. From there, each price moves about once a second using a Geometric Brownian Motion model, the same random-walk model behind options pricing.",
  },
  {
    icon: ShoppingCartIcon,
    title: "Market orders",
    body: "Orders fill immediately at the current price, with no fees. You can't spend more cash than you have, and you can only sell shares you own (no short selling).",
  },
  {
    icon: LineChartIcon,
    title: "Tracking performance",
    body: "Your portfolio is valued at live prices. Unrealized P&L compares market value to what you paid (your average cost), and all-time return compares your account value to your starting cash.",
  },
];

const SHORTCUTS = [
  { keys: "Ctrl / ⌘ + K", action: "Search stocks and pages" },
  { keys: "/", action: "Search stocks and pages" },
  { keys: "Ctrl / ⌘ + B", action: "Collapse or expand the sidebar" },
];

const Page = () => {
  return (
    <>
      <PageHeader
        title="How the simulator works"
        description="Practice trading with live, simulated prices and zero risk."
      />

      <div className="grid gap-4 md:grid-cols-2">
        {SECTIONS.map((section) => (
          <Card key={section.title}>
            <CardHeader className="flex flex-row items-start gap-3">
              <div className="rounded-lg bg-muted p-2">
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
