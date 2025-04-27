"use client";

import Link from "next/link";
import Image from "next/image";
import { usePathname } from "next/navigation";

import { Button } from "@/components/ui/button";
import { ModeToggle } from "@/components/mode-toggle";

interface AuthLayoutProps {
  children: React.ReactNode;
}

export const AuthLayout = ({ children }: AuthLayoutProps) => {
  const pathname = usePathname();

  return (
    <div className="flex flex-1 min-h-screen">
      <main className="lg:w-3/4 w-full p-8 h-screen flex flex-col">
        <header className="flex items-center justify-between">
          <div className="flex items-center justify-center">
            <Image
              src="/money.png"
              className="size-16 mr-3 object-cover hidden lg:block"
              alt="Icon"
              width={64}
              height={64}
            />
            <h1 className="text-xl lg:text-2xl font-bold">
              Stock Market Simulator
            </h1>
          </div>

          <div className="flex items-center justify-center gap-x-2">
            {pathname === "/login" && (
              <Button variant="outline" asChild>
                <Link href="/register">Sign Up</Link>
              </Button>
            )}

            {pathname === "/register" && (
              <Button variant="outline" asChild>
                <Link href="/login">Login</Link>
              </Button>
            )}

            <ModeToggle className="p-5.5" />
          </div>
        </header>

        <section className="flex flex-1 items-center">{children}</section>
      </main>

      <aside className="w-1/2 min-h-screen hidden lg:block relative">
        <Image
          src="/auth-banner.png"
          alt="Illustration"
          className="size-full"
          fill
        />
      </aside>
    </div>
  );
};
