import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import type { Role } from "@/shared/types/role";
import { ROLE_HOME } from "@/shared/lib/role-routes";

/**
 * Next.js 16 renamed the "middleware.ts" convention to "proxy.ts" (same runtime behavior,
 * new name/export to avoid confusion with Express-style middleware — see
 * node_modules/next/dist/docs/01-app/api-reference/file-conventions/proxy.md).
 *
 * This performs an OPTIMISTIC, cookie-only role check to guard routes at the edge — exactly the
 * pattern Next.js's own authentication guide recommends (read the cookie, never hit the database
 * or the backend API here). It is UX-only defense-in-depth: the backend API remains the sole
 * authoritative enforcement of role-based access (see DocsAndPlan/IMPLEMENTATION_PLAN.md section 10.1).
 */

const ROLE_PREFIXES: Array<{ prefix: string; role: Role }> = [
  { prefix: "/admin", role: "Admin" },
  { prefix: "/teacher", role: "Teacher" },
  { prefix: "/student", role: "Student" },
];

export function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl;

  const accessToken = request.cookies.get("ah_access_token")?.value;
  const role = request.cookies.get("ah_role")?.value as Role | undefined;
  const isAuthenticated = Boolean(accessToken && role);

  if (pathname === "/login") {
    if (isAuthenticated && role) {
      return NextResponse.redirect(new URL(ROLE_HOME[role], request.url));
    }
    return NextResponse.next();
  }

  if (pathname === "/") {
    return NextResponse.redirect(
      new URL(isAuthenticated && role ? ROLE_HOME[role] : "/login", request.url),
    );
  }

  const matchedRule = ROLE_PREFIXES.find((rule) => pathname.startsWith(rule.prefix));

  if (matchedRule) {
    if (!isAuthenticated) {
      const loginUrl = new URL("/login", request.url);
      loginUrl.searchParams.set("from", pathname);
      return NextResponse.redirect(loginUrl);
    }

    if (role !== matchedRule.role) {
      return NextResponse.redirect(new URL(ROLE_HOME[role!], request.url));
    }
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    /*
     * Run on every route except static assets and Next.js internals — auth guarding should
     * never accidentally block CSS/JS/images from loading.
     */
    "/((?!_next/static|_next/image|favicon.ico).*)",
  ],
};
