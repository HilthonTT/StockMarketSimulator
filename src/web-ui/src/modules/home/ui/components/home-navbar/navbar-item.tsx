import Link from "next/link";

import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export interface NavbarItemProps {
  href: string;
  children: React.ReactNode;
  isActive?: boolean;
}

export const NavbarItem = ({ href, children, isActive }: NavbarItemProps) => {
  return (
    <Button
      variant="outline"
      className={cn(
        "bg-transparent hover:bg-transparent rounded-full hover:border-primary border-transparent px-3.5 text-lg",
        isActive && "bg-black text-white hover:bg-black hover:text-white"
      )}
      aria-current={isActive ? "page" : undefined} // Indicates the current page
      asChild
    >
      <Link href={href} aria-label={children?.toString()}>
        {children}
      </Link>
    </Button>
  );
};
