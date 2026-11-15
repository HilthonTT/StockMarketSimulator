export type LeaderboardEntry = {
  rank: number;
  userId: string;
  username: string;
  accountValue: number;
  cash: number;
  holdingsValue: number;
  /** Fraction vs. the $5,000 starting cash. */
  returnPercent: number;
  positions: number;
};

export type LeaderboardResponse = {
  entries: LeaderboardEntry[];
  currentUser: LeaderboardEntry | null;
};
