import { redirect } from "next/navigation";

/**
 * "/" always redirects — proxy.ts (see src/proxy.ts) normally intercepts this before it ever
 * renders, sending authenticated users to their role dashboard and everyone else to /login.
 * This server-side redirect is a fallback in case a request ever reaches this route directly
 * (e.g. proxy's matcher is changed later and no longer covers "/").
 */
export default function RootPage() {
  redirect("/login");
}
