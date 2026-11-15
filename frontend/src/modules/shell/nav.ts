import {
  BriefcaseBusinessIcon,
  ClipboardListIcon,
  GlobeIcon,
  InfoIcon,
  LayoutDashboardIcon,
  ReceiptTextIcon,
  TrophyIcon,
  UserIcon,
  type LucideIcon,
} from "lucide-react";

export type NavItem = {
  title: string;
  href: string;
  icon: LucideIcon;
  description: string;
};

export const tradingNav: NavItem[] = [
  { title: "Dashboard", href: "/", icon: LayoutDashboardIcon, description: "Account overview" },
  { title: "Markets", href: "/markets", icon: GlobeIcon, description: "Stocks and crypto to trade" },
  { title: "Portfolio", href: "/portfolio", icon: BriefcaseBusinessIcon, description: "Your holdings and returns" },
  { title: "Orders", href: "/orders", icon: ClipboardListIcon, description: "Open limit and stop orders" },
  { title: "History", href: "/transactions", icon: ReceiptTextIcon, description: "Every trade you've made" },
  { title: "Leaderboard", href: "/leaderboard", icon: TrophyIcon, description: "Top traders by account value" },
];

export const accountNav: NavItem[] = [
  { title: "Profile", href: "/profile", icon: UserIcon, description: "Account and security settings" },
  { title: "How it works", href: "/about", icon: InfoIcon, description: "About the simulator" },
];

export const isNavActive = (pathname: string, href: string) =>
  href === "/" ? pathname === "/" : pathname === href || pathname.startsWith(`${href}/`);
