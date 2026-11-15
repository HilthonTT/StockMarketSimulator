# Stock Market Simulator: front end

Next.js 16 (App Router) client for the simulator. It talks to the .NET API through a tRPC layer on the Next server, and receives live prices from the API's SignalR hub.

## Run it

```bash
bun install
bun run dev            # http://localhost:3000
```

No `.env` is needed in development. The client calls the API at `http://localhost:5000` and uses a development-only auth secret. The API must be running (see [docs/getting-started.md](../docs/getting-started.md)).

Production (including `bun run build && bun start` on your machine) requires `AUTH_SECRET` (`bunx auth secret`), and `AUTH_TRUST_HOST=true` behind a proxy. Override the API URL with `NEXT_PUBLIC_SERVER_URL`.

| Script | What it does |
| --- | --- |
| `bun run dev` | Dev server with Turbopack |
| `bun run build` | Production build (standalone output, used by the Dockerfile) |
| `bun run lint` | ESLint (eslint-config-next flat config) |
| `bun run typecheck` | `tsc --noEmit` |

## Pages

| Route | Purpose |
| --- | --- |
| `/` | Dashboard: account value and chart, SIM 30 index, gainers/losers, news feed, holdings, recent activity |
| `/markets` | Index, top movers, the stock and crypto universe (sector filter, sparklines, day range), and search for any other symbol |
| `/stocks/[ticker]` | 1D–3M chart, key stats, your position, Market / Limit / Stop order ticket, open orders, your trades |
| `/portfolio` | Holdings at live prices, allocation donut, account value chart, day and total P&L |
| `/orders` | Open, filled and rejected limit/stop orders, with cancel |
| `/leaderboard` | Every trader ranked by account value |
| `/transactions` | Trade history with ticker and date-range filters |
| `/profile` | Username, password, avatar, banner, and Reset account |
| `/about` | How the simulation works |
| `/login`, `/register` | Authentication |

`Ctrl/⌘ + K` (or `/`) opens stock search from anywhere.

## Structure

```
src/
  app/                 Routes only: (auth) and (app) route groups, thin page files
  components/          Shared UI; components/ui is shadcn/ui
  modules/<feature>/   Feature code
    server/            tRPC procedures (call the .NET API)
    hooks/             Client hooks and zustand stores
    ui/views|sections|components
  trpc/                tRPC client/server setup
  lib/                 API client, formatting helpers
```

Key modules:

- `market`: the single SignalR connection (`StockFeedProvider`), a zustand quote store and `useLiveQuote` / `useLiveQuotes`. Hub group subscriptions are reference-counted, so many components can show the same ticker without unsubscribing each other.
- `portfolio`: `usePortfolio()` values positions at live prices and derives equity, P&L and allocation.
- `transactions`: the order ticket (`TradeTicket`), the global trade dialog and the history page.
- `shell`: sidebar, top bar, command search and ticker tape.

## Environment

See [.env.example](./.env.example). `NEXT_PUBLIC_*` values are inlined at build time, so the Docker image takes them as build arguments.
