"use client";

import { useEffect } from "react";

import { SessionLayout } from "@/modules/auth/ui/layouts/session-layout";
import { StockFeedProvider } from "@/modules/market/providers/stock-feed-provider";
import { useWatchlist } from "@/modules/market/hooks/use-watchlist";
import { TradeDialog } from "@/modules/transactions/ui/components/trade-dialog";

import { SidebarInset, SidebarProvider } from "@/components/ui/sidebar";

import { AppSidebar } from "../components/app-sidebar";
import { AppTopbar } from "../components/app-topbar";
import { CommandSearch } from "../components/command-search";
import { TickerTape } from "../components/ticker-tape";

interface AppShellProps {
  children: React.ReactNode;
  defaultSidebarOpen?: boolean;
}

/** Signed-in layout: sidebar navigation, top bar, live ticker tape and global dialogs. */
export const AppShell = ({ children, defaultSidebarOpen = true }: AppShellProps) => {
  useEffect(() => {
    void useWatchlist.persist.rehydrate();
  }, []);

  return (
    <SessionLayout>
      <StockFeedProvider>
        <SidebarProvider defaultOpen={defaultSidebarOpen}>
          <AppSidebar />
          <SidebarInset className="min-w-0">
            <AppTopbar />
            <TickerTape />
            <main className="mx-auto flex w-full max-w-7xl flex-1 flex-col gap-6 p-4 pb-16 md:p-6 lg:p-8">
              {children}
            </main>
          </SidebarInset>
        </SidebarProvider>

        <TradeDialog />
        <CommandSearch />
      </StockFeedProvider>
    </SessionLayout>
  );
};
