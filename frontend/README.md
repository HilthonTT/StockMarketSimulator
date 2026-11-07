# Stock Market Simulator: front end

Next.js 16 (App Router) client for the simulator. It talks to the .NET API through a tRPC layer on the Next server, and receives live prices from the API's SignalR hub.

## Run it

```bash
cp .env.example .env   # then set AUTH_SECRET (bunx auth secret)
bun install
bun run dev            # http://localhost:3000
```

The API must be running (see [docs/getting-started.md](../docs/getting-started.md)).

| Script | What it does |
| --- | --- |
| `bun run dev` | Dev server with Turbopack |
| `bun run build` | Production build (standalone output, used by the Dockerfile) |
| `bun run lint` | ESLint (eslint-config-next flat config) |
| `bun run typecheck` | `tsc --noEmit` |

## Pages

| Route | Purpose |
| --- | --- |
| `/` | Dashboard: account value, buying power, live chart of a holding, top holdings, recent activity |
| `/markets` | Search companies by ticker or name, add to watchlist, open a trade |
| `/stocks/[ticker]` | Live price chart, your position, order ticket, your trades in that stock |
| `/portfolio` | Holdings at live prices, total return, allocation |
| `/transactions` | Order history with ticker and date-range filters |
| `/profile` | Username, password, avatar and banner |
| `/about` | How the simulation works, keyboard shortcuts |
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
