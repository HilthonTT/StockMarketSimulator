import type { Metadata } from "next";
import { DM_Sans } from "next/font/google";
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
});

export const metadata: Metadata = {
  title: "Stock Market Simulator | Practice Trading Risk-Free",
  description:
    "Master the stock market without the risk. Simulate real-world trading, test investment strategies, and track performance — all in a realistic trading environment.",
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
  icons: ["/money.png"],
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
        <body className={cn("antialiased", dmSans.className)}>
          <ThemeProvider
            attribute="class"
            defaultTheme="system"
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
