import type { Metadata } from "next";

import { PortfolioView } from "@/modules/portfolio/ui/views/portfolio-view";

export const metadata: Metadata = { title: "Portfolio" };

// Positions and cash are prefetched by the (app) layout.
const Page = () => <PortfolioView />;

export default Page;
