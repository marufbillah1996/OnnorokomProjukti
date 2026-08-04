import type { Role } from "@/shared/types/role";

/** Single source of truth for "where does this role land after login" — used by proxy.ts,
 * useRoleGuard, and the login form so the mapping is never duplicated. */
export const ROLE_HOME: Record<Role, string> = {
  Admin: "/admin/dashboard",
  Teacher: "/teacher/dashboard",
  Student: "/student/dashboard",
};
