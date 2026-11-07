import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

import { SERVER_URL } from "@/constants";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

const currencyFormatter = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

const compactCurrencyFormatter = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
  notation: "compact",
  maximumFractionDigits: 2,
});

const percentFormatter = new Intl.NumberFormat("en-US", {
  style: "percent",
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

const numberFormatter = new Intl.NumberFormat("en-US");

export function formatCurrency(value: number): string {
  return currencyFormatter.format(value);
}

export function formatCompactCurrency(value: number): string {
  return compactCurrencyFormatter.format(value);
}

/** `+$12.30` / `-$4.00`; zero has no sign. */
export function formatSignedCurrency(value: number): string {
  const sign = value > 0 ? "+" : value < 0 ? "-" : "";

  return `${sign}${currencyFormatter.format(Math.abs(value))}`;
}

/** Formats a ratio (`0.0123` → `+1.23%`). */
export function formatPercent(
  ratio: number,
  { signed = true }: { signed?: boolean } = {}
): string {
  const formatted = percentFormatter.format(Math.abs(ratio));

  if (!signed) {
    return percentFormatter.format(ratio);
  }

  const sign = ratio > 0 ? "+" : ratio < 0 ? "-" : "";

  return `${sign}${formatted}`;
}

export function formatNumber(value: number): string {
  return numberFormatter.format(value);
}

/** Safe ratio: returns 0 instead of `NaN`/`Infinity` for a zero base. */
export function ratio(part: number, whole: number): number {
  return whole === 0 ? 0 : part / whole;
}

export function getImageUrl(imageId: string | null | undefined): string {
  if (!imageId) {
    return "";
  }

  return `${SERVER_URL}/api/v1/files/${imageId}`;
}

/**
 * Parses an API `*OnUtc` timestamp. EF Core returns UTC values without an offset
 * (`2025-01-01T12:00:00`), which JavaScript would otherwise read as local time.
 */
export function parseUtc(value: string | Date): Date {
  if (value instanceof Date) {
    return value;
  }

  return new Date(/[zZ]|[+-]\d{2}:?\d{2}$/.test(value) ? value : `${value}Z`);
}
