"use client";

import { getStoredName } from "@/shared/lib/auth-storage";
import { useRoleGuard } from "@/shared/hooks/useRoleGuard";
import { Header } from "@/shared/components/ui/Header";
import { Sidebar } from "@/shared/components/ui/Sidebar";
import { NotificationBell } from "@/features/notifications/components/NotificationBell";
import { ToastListener } from "@/features/notifications/components/ToastListener";
import { useLogout } from "@/features/auth/hooks";

const SIDEBAR_ITEMS = [
  { href: "/teacher/dashboard", label: "Dashboard" },
  { href: "/teacher/assignments", label: "Assignments" },
];

export default function TeacherLayout({ children }: { children: React.ReactNode }) {
  useRoleGuard("Teacher");

  const logout = useLogout();

  return (
    <div className="flex min-h-screen">
      <Sidebar roleLabel="Teacher" items={SIDEBAR_ITEMS} />
      <div className="flex flex-1 flex-col">
        <Header
          title="Teacher"
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
