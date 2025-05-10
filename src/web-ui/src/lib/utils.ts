import { SERVER_URL } from "@/constants";
import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatPercentage(
  value: number,
  options: {
    addPrefix?: boolean;
  } = {
    addPrefix: false,
  }
): string {
  const result = new Intl.NumberFormat("en-US", {
    style: "percent",
  }).format(value / 100);

  if (options.addPrefix && value > 0) {
    return `+${result}`;
  }

  return result;
}

export function formatCurrency(value: number): string {
  return Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    minimumFractionDigits: 2,
  }).format(value);
}

export function getImageUrl(imageId: string): string {
  if (!imageId) {
    return "";
  }

  return `${SERVER_URL}/api/v1/files/${imageId}`;
}
