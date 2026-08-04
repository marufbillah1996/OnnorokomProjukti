"use client";

import { useState } from "react";
import { QueryClientProvider } from "@tanstack/react-query";
import { createQueryClient } from "@/shared/lib/query-client";

export function Providers({ children }: { children: React.ReactNode }) {
  // Created once per browser session via useState's lazy initializer — never re-created on
  // re-render, and never shared across requests on the server (each request gets its own
  // component tree), avoiding the classic "cross-request cache leakage" pitfall with
  // TanStack Query + the App Router.
  const [queryClient] = useState(() => createQueryClient());

  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}
