"use client";

import Link from "next/link";
import Image from "next/image";
import { ActivityIcon, BadgeDollarSignIcon, ShieldCheckIcon } from "lucide-react";

import { formatCurrency } from "@/lib/utils";
import { STARTING_CASH } from "@/constants";
import { ModeToggle } from "@/components/mode-toggle";

interface AuthLayoutProps {
  children: React.ReactNode;
}

const HIGHLIGHTS = [
  { icon: BadgeDollarSignIcon, text: `Start with ${formatCurrency(STARTING_CASH)} in virtual cash` },
  { icon: ActivityIcon, text: "Prices move every second, seeded from real quotes" },
  { icon: ShieldCheckIcon, text: "Learn to trade without risking a cent" },
];

export const AuthLayout = ({ children }: AuthLayoutProps) => {
  return (
    <div className="grid min-h-svh lg:grid-cols-2">
      <div className="flex flex-col gap-6 p-6 md:p-10">
        <header className="flex items-center justify-between">
          <Link href="/" className="flex items-center gap-2 font-semibold">
            <Image src="/money.png" alt="" width={32} height={32} className="size-8 rounded-md" />
            Stock Market Simulator
          </Link>
          <ModeToggle />
        </header>

        <main className="flex flex-1 items-center justify-center">
          <div className="w-full max-w-sm">{children}</div>
        </main>
      </div>

      <aside className="relative hidden overflow-hidden bg-muted lg:block">
        <Image src="/auth-banner.png" alt="" fill priority className="object-cover" />
        <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/30 to-transparent" />
        <div className="absolute inset-x-0 bottom-0 flex flex-col gap-4 p-10 text-white">
          <h2 className="text-3xl font-bold leading-tight">Practice the market. Keep your money.</h2>
          <ul className="flex flex-col gap-2">
            {HIGHLIGHTS.map((highlight) => (
              <li key={highlight.text} className="flex items-center gap-2 text-white/90">
                <highlight.icon className="size-4 shrink-0" aria-hidden="true" />
                {highlight.text}
              </li>
            ))}
          </ul>
        </div>
      </aside>
    </div>
  );
};
