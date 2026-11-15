import type { Metadata } from "next";
import { DM_Sans, Geist_Mono } from "next/font/google";
import { SessionProvider } from "next-auth/react";
import { NuqsAdapter } from "nuqs/adapters/next/app";
import "./globals.css";

import { auth } from "@/modules/auth/auth";

import { cn } from "@/lib/utils";
import { ThemeProvider } from "@/components/providers/theme-provider";
import { Toaster } from "@/components/ui/sonner";
import { TRPCReactProvider } from "@/trpc/client";

const dmSans = DM_Sans({
  subsets: ["latin"],
  variable: "--font-dm-sans",
});

const geistMono = Geist_Mono({
  subsets: ["latin"],
  variable: "--font-geist-mono",
});

export const metadata: Metadata = {
  title: {
    default: "StockMarketSimulator · Paper trading for stocks & crypto",
    template: "%s · StockMarketSimulator",
  },
  description:
    "Paper-trade stocks and crypto with $5,000 of virtual cash: live simulated prices, limit and stop orders, market news and a leaderboard.",
  keywords: [
    "stock market simulator",
    "paper trading",
    "investment simulator",
    "trading practice",
    "learn stock trading",
    "virtual trading",
    "simulate investing",
    "risk-free trading",
    "stock trading game",
    "portfolio simulation",
  ],
  authors: [{ name: "Hilthon", url: "https://hilthon.vercel.app" }],
  applicationName: "StockMarketSimulator",
};

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const session = await auth();

  return (
    <SessionProvider session={session}>
      <html lang="en" suppressHydrationWarning>
        <body className={cn("antialiased", dmSans.variable, geistMono.variable)}>
          <ThemeProvider
            attribute="class"
            defaultTheme="dark"
            enableSystem
            disableTransitionOnChange
          >
            <NuqsAdapter>
              <TRPCReactProvider>
                <Toaster />
                {children}
              </TRPCReactProvider>
            </NuqsAdapter>
          </ThemeProvider>
        </body>
      </html>
    </SessionProvider>
  );
}
