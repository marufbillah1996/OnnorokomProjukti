/**
 * @jest-environment node
 *
 * proxy.ts constructs real Web-standard Request/Response objects (via next/server's
 * NextRequest/NextResponse, which extend the global `Request`/`Response`). jsdom deliberately
 * does not implement the fetch API (no global Request/Response/Headers), so this file overrides
 * the environment to Node for just this suite — Node has provided those globals natively since
 * v18 — while the rest of the test suite keeps the project-wide jsdom environment for rendering
 * React components.
 */
import { NextRequest } from "next/server";
import { proxy } from "@/proxy";

function buildRequest(path: string, cookie?: string) {
  const url = new URL(path, "http://localhost:3000");
  const headers = new Headers();
  if (cookie) {
    headers.set("cookie", cookie);
  }
  return new NextRequest(url, { headers });
}

describe("proxy (route guard)", () => {
  it("redirects an unauthenticated request to a role-guarded route to /login", () => {
    const request = buildRequest("/admin/dashboard");

    const response = proxy(request);

    expect(response.status).toBe(307);
    expect(response.headers.get("location")).toContain("/login");
  });

  it("redirects a wrong-role request to that role's own home instead of the requested route", () => {
    const request = buildRequest(
      "/admin/dashboard",
      "ah_access_token=faketoken; ah_role=Student",
    );

    const response = proxy(request);

    const location = response.headers.get("location");
    expect(response.status).toBe(307);
    expect(location).toContain("/student/dashboard");
    expect(location).not.toContain("/admin/dashboard");
  });

  it("lets a correctly-authenticated, correctly-roled request through with no redirect", () => {
    const request = buildRequest(
      "/teacher/dashboard",
      "ah_access_token=faketoken; ah_role=Teacher",
    );

    const response = proxy(request);

    expect(response.headers.get("location")).toBeNull();
    expect(response.status).toBe(200);
  });
});
