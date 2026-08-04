/**
 * Passthrough layout for the "(auth)" route group. Route groups don't add a URL segment, so
 * this exists only to satisfy the App Router convention of every route group having its own
 * layout file. app/layout.tsx already provides the full HTML shell and providers.
 */
export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return children;
}
