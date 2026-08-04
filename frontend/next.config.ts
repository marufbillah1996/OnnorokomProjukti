import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Produces a self-contained .next/standalone build (minimal node_modules copied in) —
  // dramatically smaller Docker images than copying the full node_modules tree.
  output: "standalone",
};

export default nextConfig;
