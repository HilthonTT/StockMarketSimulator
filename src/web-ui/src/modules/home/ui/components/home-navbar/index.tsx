"use client";

import Link from "next/link";
import { Poppins } from "next/font/google";
import { usePathname } from "next/navigation";
import { useState } from "react";
import { MenuIcon, MoonIcon, SunIcon } from "lucide-react";
import { useTheme } from "next-themes";

import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";

import { NavbarItem } from "./navbar-item";
import { HomeNavbarSidebar } from "./home-navbar-sidebar";

const poppins = Poppins({
  subsets: ["latin"],
  weight: ["700"],
});

const navbarItems = [
  { href: "/", children: "Home" },
  { href: "/about", children: "About" },
  { href: "/Profile", children: "Profile" },
  { href: "/contact", children: "Contact" },
];

export const HomeNavbar = () => {
  const pathname = usePathname();
  const { setTheme, theme } = useTheme();

  const [isSidebarOpen, setIsSidebarOpen] = useState(false);

  return (
    <nav className="h-20 flex border-b justify-between font-medium bg-white dark:bg-black">
      <Link href="/" className="pl-6 flex items-center">
        <span
          className={cn(
            "text-xl lg:text-3xl xl:text-4xl 2xl:text-5xl font-semibold",
            poppins.className
          )}
        >
          Stock Market Simulator
        </span>
      </Link>

      <HomeNavbarSidebar
        items={navbarItems}
        open={isSidebarOpen}
        onOpenChange={setIsSidebarOpen}
      />

      <div className="items-center gap-4 hidden lg:flex">
        {navbarItems.map((item) => (
          <NavbarItem
            key={item.href}
            href={item.href}
            isActive={pathname === item.href}
          >
            {item.children}
          </NavbarItem>
        ))}
      </div>

      <div className="hidden lg:flex">
        <Button
          asChild
          variant="secondary"
          className="border-l border-t-0 border-b-0 border-r-0 px-12 h-full rounded-none bg-white dark:bg-black hover:bg-pink-400 transition-colors text-lg"
        >
          <Link href="/login">Log in</Link>
        </Button>
        <Button
          asChild
          variant="secondary"
          className="border-l border-t-0 border-b-0 border-r-0 px-12 h-full rounded-none bg-black text-white hover:bg-pink-400  hover:text-black transition-colors text-lg"
        >
          <Link href="/register">Start Gambling!</Link>
        </Button>

        <Button
          variant="secondary"
          className="relative border-l border-t-0 border-b-0 border-r-0 px-12 size-24 h-full rounded-none bg-black text-white hover:bg-pink-400  hover:text-black transition-colors text-lg"
        >
          <div
            onClick={() => {
              if (theme === "dark") {
                setTheme("light");
              } else {
                setTheme("dark");
              }
            }}
            className="absolute inset-0 flex items-center justify-center"
          >
            <MoonIcon className="hidden dark:block size-6" />
            <SunIcon className="block dark:hidden size-6" />
          </div>
        </Button>
      </div>

      <div className="flex lg:hidden items-center justify-center">
        <Button
          variant="ghost"
          className="size-12 border-transparent bg-white dark:bg-black"
          onClick={() => setIsSidebarOpen(true)}
        >
          <MenuIcon />
        </Button>
      </div>
    </nav>
  );
};
