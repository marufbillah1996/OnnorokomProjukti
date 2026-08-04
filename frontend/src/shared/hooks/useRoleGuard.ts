"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { getAccessToken, getStoredRole } from "@/shared/lib/auth-storage";
import { ROLE_HOME } from "@/shared/lib/role-routes";
import type { Role } from "@/shared/types/role";

/**
 * Client-side defense-in-depth on top of proxy.ts's edge-level redirect (see src/proxy.ts) —
 * catches the case where a client-side transition renders a role layout without a fresh
 * server round-trip. The backend API remains the only authoritative check either way.
 */
export function useRoleGuard(requiredRole: Role) {
  const router = useRouter();

  useEffect(() => {
    const token = getAccessToken();
    const role = getStoredRole();

    if (!token || !role) {
      router.replace("/login");
      return;
    }

    if (role !== requiredRole) {
      router.replace(ROLE_HOME[role]);
    }
  }, [requiredRole, router]);
}
