import Cookies from "js-cookie";
import type { Role } from "@/shared/types/role";

/**
 * Client-side (browser) token storage, backed by cookies rather than localStorage so that
 * Next.js middleware (which runs server-side and can only read cookies, never localStorage)
 * can guard routes by role without an extra round-trip.
 *
 * Design tradeoff (documented in README "Design decisions"): these cookies are NOT httpOnly.
 * A true httpOnly refresh-token cookie would require every API call to be proxied through a
 * Next.js Route Handler (a "BFF" pattern) since the browser cannot attach an httpOnly cookie's
 * value to an Authorization header itself. Given this project's scope, we accept the same
 * XSS-exposure tradeoff most SPAs already accept with localStorage-held JWTs, and mitigate it
 * with a short-lived access token (15 min) plus refresh-token rotation on every use.
 */
const ACCESS_TOKEN_COOKIE = "ah_access_token";
const REFRESH_TOKEN_COOKIE = "ah_refresh_token";
const ROLE_COOKIE = "ah_role";
const NAME_COOKIE = "ah_name";

export interface StoredSession {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
  role: Role;
  name: string;
}

const cookieOptions: Cookies.CookieAttributes = {
  sameSite: "lax",
  secure: typeof window !== "undefined" && window.location.protocol === "https:",
  path: "/",
};

export function saveSession(session: StoredSession): void {
  Cookies.set(ACCESS_TOKEN_COOKIE, session.accessToken, {
    ...cookieOptions,
    expires: new Date(session.accessTokenExpiresAtUtc),
  });
  Cookies.set(REFRESH_TOKEN_COOKIE, session.refreshToken, {
    ...cookieOptions,
    expires: new Date(session.refreshTokenExpiresAtUtc),
  });
  Cookies.set(ROLE_COOKIE, session.role, {
    ...cookieOptions,
    expires: new Date(session.refreshTokenExpiresAtUtc),
  });
  Cookies.set(NAME_COOKIE, session.name, {
    ...cookieOptions,
    expires: new Date(session.refreshTokenExpiresAtUtc),
  });
}

export function getAccessToken(): string | undefined {
  return Cookies.get(ACCESS_TOKEN_COOKIE);
}

export function getRefreshToken(): string | undefined {
  return Cookies.get(REFRESH_TOKEN_COOKIE);
}

export function getStoredRole(): Role | undefined {
  return Cookies.get(ROLE_COOKIE) as Role | undefined;
}

export function getStoredName(): string | undefined {
  return Cookies.get(NAME_COOKIE);
}

export function clearSession(): void {
  Cookies.remove(ACCESS_TOKEN_COOKIE, { path: "/" });
  Cookies.remove(REFRESH_TOKEN_COOKIE, { path: "/" });
  Cookies.remove(ROLE_COOKIE, { path: "/" });
  Cookies.remove(NAME_COOKIE, { path: "/" });
}
