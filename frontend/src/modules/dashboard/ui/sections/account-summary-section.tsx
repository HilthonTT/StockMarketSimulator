"use client";

import { BriefcaseBusinessIcon, LandmarkIcon, TrendingUpIcon, WalletIcon } from "lucide-react";

import { usePortfolio } from "@/modules/portfolio/hooks/use-portfolio";
import { PriceChange } from "@/modules/market/ui/components/price-change";

import { formatCurrency, formatPercent } from "@/lib/utils";
import { STARTING_CASH } from "@/constants";
import { QueryBoundary } from "@/components/query-boundary";
import { StatCard, StatCardSkeleton } from "@/components/stat-card";

export const AccountSummarySection = () => (
  <QueryBoundary
    errorTitle="Couldn't load your account"
    fallback={
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {Array.from({ length: 4 }, (_, index) => (
          <StatCardSkeleton key={index} />
        ))}
      </div>
    }
  >
    <AccountSummary />
  </QueryBoundary>
);

const AccountSummary = () => {
  const portfolio = usePortfolio();

  return (
    <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      <StatCard
        label="Account value"
        icon={LandmarkIcon}
        value={formatCurrency(portfolio.equity)}
        footer={
          <span className="inline-flex flex-wrap items-center gap-1">
            <PriceChange change={portfolio.allTimePnl} ratio={portfolio.allTimePnlRatio} />
            <span>all time</span>
          </span>
        }
      />
      <StatCard
        label="Buying power"
        icon={WalletIcon}
        value={formatCurrency(portfolio.cash)}
        footer={`${formatPercent(portfolio.cashRatio, { signed: false })} of your account is cash`}
      />
      <StatCard
        label="Invested"
        icon={BriefcaseBusinessIcon}
        value={formatCurrency(portfolio.investedValue)}
        footer={
          portfolio.holdings.length === 0
            ? "No open positions"
            : `${portfolio.holdings.length} position${portfolio.holdings.length === 1 ? "" : "s"} · cost ${formatCurrency(portfolio.costBasis)}`
        }
      />
      <StatCard
        label="Unrealized P&L"
        icon={TrendingUpIcon}
        value={<PriceChange change={portfolio.unrealizedPnl} showIcon={false} className="text-2xl font-bold" />}
        footer={
          portfolio.holdings.length === 0
            ? `You started with ${formatCurrency(STARTING_CASH)}`
            : (
              <span className="inline-flex flex-wrap items-center gap-1">
                <PriceChange change={portfolio.sessionPnl} />
                <span>this session</span>
              </span>
            )
        }
      />
    </div>
  );
};
