"use client";

import { useRoleGuard } from "@/shared/hooks/useRoleGuard";
import { Sidebar } from "@/shared/components/ui/Sidebar";
import { Header } from "@/shared/components/ui/Header";
import { getStoredName } from "@/shared/lib/auth-storage";
import { useLogout } from "@/features/auth/hooks";
import { NotificationBell } from "@/features/notifications/components/NotificationBell";
import { ToastListener } from "@/features/notifications/components/ToastListener";

const NAV_ITEMS = [
  { href: "/student/dashboard", label: "Dashboard" },
  { href: "/student/assignments", label: "Assignments" },
];

export default function StudentLayout({ children }: { children: React.ReactNode }) {
  useRoleGuard("Student");
  const logout = useLogout();

  return (
    <div className="flex min-h-screen">
      <Sidebar roleLabel="Student" items={NAV_ITEMS} />
      <div className="flex flex-1 flex-col">
        <Header
          title="Student"
          userName={getStoredName() ?? ""}
          onLogout={() => logout.mutate()}
          actions={<NotificationBell />}
        />
        <main className="flex-1 bg-background p-6">{children}</main>
      </div>
      <ToastListener />
    </div>
  );
}
