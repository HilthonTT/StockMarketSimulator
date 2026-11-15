import Link from "next/link";
import { ActivityIcon, BadgeDollarSignIcon, BitcoinIcon, TrophyIcon } from "lucide-react";

import { cn, formatCurrency } from "@/lib/utils";
import { STARTING_CASH } from "@/constants";
import { Logo } from "@/components/logo";
import { ModeToggle } from "@/components/mode-toggle";

interface AuthLayoutProps {
  children: React.ReactNode;
}

const HIGHLIGHTS = [
  { icon: BadgeDollarSignIcon, text: `Start with ${formatCurrency(STARTING_CASH)} in virtual cash` },
  { icon: ActivityIcon, text: "Stocks tick every second, with news that moves prices" },
  { icon: BitcoinIcon, text: "Trade crypto around the clock" },
  { icon: TrophyIcon, text: "Limit and stop orders, and a live leaderboard" },
];

// Decorative sample rows for the terminal preview (not live data).
const PREVIEW_ROWS = [
  { symbol: "NVDA", price: "131.42", change: 2.31 },
  { symbol: "AAPL", price: "227.18", change: 0.84 },
  { symbol: "ETH", price: "3,412.50", change: -1.12 },
  { symbol: "TSLA", price: "248.03", change: 3.95 },
  { symbol: "JPM", price: "214.66", change: -0.37 },
];

const PREVIEW_PATH =
  "M0 92 L18 88 L36 90 L54 80 L72 84 L90 70 L108 74 L126 62 L144 66 L162 52 L180 58 L198 44 L216 48 L234 34 L252 38 L270 26 L288 30 L306 18 L320 22";

export const AuthLayout = ({ children }: AuthLayoutProps) => {
  return (
    <div className="grid min-h-svh lg:grid-cols-[minmax(0,1fr)_minmax(0,1.1fr)]">
      <div className="flex flex-col gap-6 p-6 md:p-10">
        <header className="flex items-center justify-between">
          <Link href="/" aria-label="StockMarketSimulator home">
            <Logo />
          </Link>
          <ModeToggle />
        </header>

        <main className="flex flex-1 items-center justify-center">
          <div className="w-full max-w-sm">{children}</div>
        </main>

        <p className="text-center text-xs text-muted-foreground">
          A paper-trading simulator. No real money, no real orders.
        </p>
      </div>

      <aside className="relative hidden overflow-hidden border-l bg-sidebar lg:flex lg:flex-col lg:justify-center lg:gap-10 lg:p-12">
        <div
          className="pointer-events-none absolute inset-0 opacity-60 [background-image:radial-gradient(circle_at_20%_10%,color-mix(in_oklch,var(--primary)_25%,transparent),transparent_45%),radial-gradient(circle_at_90%_90%,color-mix(in_oklch,var(--gain)_18%,transparent),transparent_40%)]"
          aria-hidden="true"
        />

        <div className="relative flex flex-col gap-4">
          <h2 className="max-w-md text-4xl font-bold leading-tight tracking-tight">
            Practice the market. <span className="text-primary">Keep your money.</span>
          </h2>
          <ul className="flex flex-col gap-2.5">
            {HIGHLIGHTS.map((highlight) => (
              <li key={highlight.text} className="flex items-center gap-2.5 text-muted-foreground">
                <span className="rounded-md bg-primary/15 p-1.5 text-primary">
                  <highlight.icon className="size-4 shrink-0" aria-hidden="true" />
                </span>
                {highlight.text}
              </li>
            ))}
          </ul>
        </div>

        {/* Terminal preview: purely decorative. */}
        <div className="relative max-w-lg rounded-xl border bg-card/80 shadow-2xl backdrop-blur" aria-hidden="true">
          <div className="flex items-center gap-1.5 border-b px-4 py-2.5">
            <span className="size-2.5 rounded-full bg-loss/70" />
            <span className="size-2.5 rounded-full bg-amber-400/70" />
            <span className="size-2.5 rounded-full bg-gain/70" />
            <span className="ml-3 text-xs text-muted-foreground">SIM 30 · live</span>
          </div>
          <div className="grid grid-cols-[1fr_auto] gap-4 p-4">
            <svg viewBox="0 0 320 100" className="h-28 w-full" preserveAspectRatio="none">
              <defs>
                <linearGradient id="auth-preview-fill" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="var(--gain)" stopOpacity="0.35" />
                  <stop offset="100%" stopColor="var(--gain)" stopOpacity="0" />
                </linearGradient>
              </defs>
              <path d={`${PREVIEW_PATH} L320 100 L0 100 Z`} fill="url(#auth-preview-fill)" />
              <path d={PREVIEW_PATH} fill="none" stroke="var(--gain)" strokeWidth="2" vectorEffect="non-scaling-stroke" />
            </svg>
            <ul className="flex min-w-36 flex-col gap-1.5 text-xs">
              {PREVIEW_ROWS.map((row) => (
                <li key={row.symbol} className="flex items-center justify-between gap-3">
                  <span className="font-semibold">{row.symbol}</span>
                  <span className="num text-muted-foreground">{row.price}</span>
                  <span className={cn("num w-12 text-right", row.change >= 0 ? "text-gain" : "text-loss")}>
                    {row.change >= 0 ? "+" : ""}
                    {row.change.toFixed(2)}%
                  </span>
                </li>
              ))}
            </ul>
          </div>
        </div>
      </aside>
    </div>
  );
};
