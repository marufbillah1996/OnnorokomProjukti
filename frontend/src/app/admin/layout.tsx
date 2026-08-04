"use client";

import { useRoleGuard } from "@/shared/hooks/useRoleGuard";
import { Sidebar } from "@/shared/components/ui/Sidebar";
import { Header } from "@/shared/components/ui/Header";
import { getStoredName } from "@/shared/lib/auth-storage";
import { useLogout } from "@/features/auth/hooks";
import { NotificationBell } from "@/features/notifications/components/NotificationBell";
import { ToastListener } from "@/features/notifications/components/ToastListener";

const SIDEBAR_ITEMS = [
  { href: "/admin/dashboard", label: "Dashboard" },
  { href: "/admin/users", label: "Users" },
  { href: "/admin/academics", label: "Academics" },
  { href: "/admin/monitoring", label: "Monitoring" },
];

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  useRoleGuard("Admin");

  const logout = useLogout();

  return (
    <div className="flex h-screen">
      <ToastListener />
      <Sidebar roleLabel="Admin" items={SIDEBAR_ITEMS} />
      <div className="flex flex-1 flex-col overflow-hidden">
        <Header
          title="Admin"
          userName={getStoredName() ?? "Admin"}
          onLogout={() => logout.mutate()}
          actions={<NotificationBell />}
        />
        <main className="flex-1 overflow-y-auto bg-background p-6">{children}</main>
      </div>
    </div>
  );
}
