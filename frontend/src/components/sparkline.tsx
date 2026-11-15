import { useId } from "react";

import { cn } from "@/lib/utils";

interface SparklineProps {
  values: number[];
  /** Colour by this baseline (e.g. the previous close) instead of the first value. */
  baseline?: number;
  width?: number;
  height?: number;
  className?: string;
}

/** Tiny trend line for dense tables. Purely decorative (the numbers sit next to it). */
export const Sparkline = ({ values, baseline, width = 96, height = 28, className }: SparklineProps) => {
  const gradientId = useId();

  if (values.length < 2) {
    return <span className={cn("inline-block", className)} style={{ width, height }} aria-hidden="true" />;
  }

  const min = Math.min(...values, baseline ?? Infinity);
  const max = Math.max(...values, baseline ?? -Infinity);
  const span = max - min || 1;
  const step = width / (values.length - 1);
  const y = (value: number) => height - 2 - ((value - min) / span) * (height - 4);

  const points = values.map((value, index) => `${(index * step).toFixed(1)},${y(value).toFixed(1)}`);
  const up = values[values.length - 1] >= (baseline ?? values[0]);
  const color = up ? "var(--gain)" : "var(--loss)";

  return (
    <svg
      width={width}
      height={height}
      viewBox={`0 0 ${width} ${height}`}
      className={cn("overflow-visible", className)}
      aria-hidden="true"
    >
      <defs>
        <linearGradient id={gradientId} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor={color} stopOpacity={0.25} />
          <stop offset="100%" stopColor={color} stopOpacity={0} />
        </linearGradient>
      </defs>
      {baseline !== undefined && (
        <line
          x1={0}
          x2={width}
          y1={y(baseline)}
          y2={y(baseline)}
          stroke="var(--muted-foreground)"
          strokeOpacity={0.4}
          strokeDasharray="2 3"
        />
      )}
      <polygon points={`0,${height} ${points.join(" ")} ${width},${height}`} fill={`url(#${gradientId})`} />
      <polyline points={points.join(" ")} fill="none" stroke={color} strokeWidth={1.5} strokeLinejoin="round" />
    </svg>
  );
};
