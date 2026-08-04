"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import clsx from "clsx";

export interface SidebarNavItem {
  href: string;
  label: string;
}

export interface SidebarProps {
  roleLabel: string;
  items: SidebarNavItem[];
}

export function Sidebar({ roleLabel, items }: SidebarProps) {
  const pathname = usePathname();

  return (
    <aside className="hidden w-64 shrink-0 flex-col border-r border-border bg-surface md:flex">
      <div className="border-b border-border px-5 py-4">
        <p className="text-lg font-semibold text-foreground">AssignmentHub</p>
        <p className="text-xs text-foreground/60">{roleLabel}</p>
      </div>
      <nav className="flex flex-1 flex-col gap-1 p-3">
        {items.map((item) => {
          const isActive = pathname === item.href || pathname.startsWith(`${item.href}/`);
          return (
            <Link
              key={item.href}
              href={item.href}
              className={clsx(
                "rounded-md px-3 py-2 text-sm font-medium transition-colors",
                isActive
                  ? "bg-brand-50 text-brand-700"
                  : "text-foreground/70 hover:bg-surface-muted",
              )}
            >
              {item.label}
            </Link>
          );
        })}
      </nav>
    </aside>
  );
}
