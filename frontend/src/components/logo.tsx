import { useId } from "react";

import { cn } from "@/lib/utils";

/** The StockMarketSimulator mark: ascending candles with a breakout trend line. */
export const LogoMark = ({ className }: { className?: string }) => {
  const gradientId = useId();

  return (
    <svg viewBox="0 0 64 64" className={cn("size-8 shrink-0", className)} aria-hidden="true">
      <defs>
        <linearGradient id={gradientId} x1="8" y1="4" x2="58" y2="62" gradientUnits="userSpaceOnUse">
          <stop offset="0" stopColor="#7C6CFF" />
          <stop offset="1" stopColor="#2E3BD6" />
        </linearGradient>
      </defs>
      <rect width="64" height="64" rx="15" fill={`url(#${gradientId})`} />
      <path
        d="M17 30v20M32 21v24M47 12v22"
        stroke="#fff"
        strokeOpacity=".55"
        strokeWidth="2.5"
        strokeLinecap="round"
      />
      <rect x="12.5" y="35" width="9" height="11" rx="2.2" fill="#fff" fillOpacity=".9" />
      <rect x="27.5" y="26" width="9" height="14" rx="2.2" fill="#fff" fillOpacity=".9" />
      <rect x="42.5" y="16" width="9" height="13" rx="2.2" fill="#3DF2A0" />
      <path
        d="M10 49 L24 40 L34 43 L52 24"
        fill="none"
        stroke="#3DF2A0"
        strokeWidth="3.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
};

/** Mark + word mark, with an optional tagline under it. */
export const Logo = ({ className, subtitle }: { className?: string; subtitle?: string }) => (
  <span className={cn("flex items-center gap-2.5", className)}>
    <LogoMark />
    <span className="grid leading-tight">
      <span className="truncate text-[15px] font-bold tracking-tight">
        StockMarket<span className="text-primary">Simulator</span>
      </span>
      {subtitle && <span className="truncate text-[11px] text-muted-foreground">{subtitle}</span>}
    </span>
  </span>
);
