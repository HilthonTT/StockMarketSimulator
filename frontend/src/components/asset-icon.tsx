import { cn } from "@/lib/utils";
import { displaySymbol, isCryptoTicker, tickerHue } from "@/lib/assets";

interface AssetIconProps {
  ticker: string;
  /** Overrides detection from the ticker (`ETH-USD` is crypto). */
  assetClass?: "stock" | "crypto";
  size?: "sm" | "md" | "lg";
  className?: string;
}

const SIZES = {
  sm: "size-7 text-[10px]",
  md: "size-9 text-xs",
  lg: "size-12 text-sm",
};

/**
 * Monogram for an asset: stocks get a rounded square, crypto a coin with a ring.
 * Colours are derived from the ticker so the same asset always looks the same.
 */
export const AssetIcon = ({ ticker, assetClass, size = "md", className }: AssetIconProps) => {
  const isCrypto = (assetClass ?? (isCryptoTicker(ticker) ? "crypto" : "stock")) === "crypto";
  const symbol = displaySymbol(ticker.toUpperCase());
  const hue = isCrypto ? 75 : tickerHue(symbol);
  const label = symbol.slice(0, 4);

  return (
    <span
      aria-hidden="true"
      className={cn(
        "relative inline-flex shrink-0 select-none items-center justify-center font-bold tracking-tight",
        isCrypto ? "rounded-full ring-2 ring-inset" : "rounded-lg",
        SIZES[size],
        label.length > 3 && "text-[0.6rem]",
        className
      )}
      style={{
        backgroundColor: `oklch(0.62 0.14 ${hue} / 0.18)`,
        color: `oklch(0.72 0.15 ${hue})`,
        ...(isCrypto ? { "--tw-ring-color": `oklch(0.75 0.15 ${hue} / 0.55)` } : {}),
      } as React.CSSProperties}
    >
      {label}
    </span>
  );
};
