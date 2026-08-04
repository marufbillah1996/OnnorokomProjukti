import { defineConfig, globalIgnores } from "eslint/config";
import nextVitals from "eslint-config-next/core-web-vitals";
import nextTs from "eslint-config-next/typescript";
import boundaries from "eslint-plugin-boundaries";

/**
 * Enforces the layered frontend architecture from IMPLEMENTATION_PLAN.md section 4:
 *   app/**      -> may import features/** and shared/**
 *   features/** -> may import shared/** and files within the SAME feature only
 *   shared/**   -> may import nothing from features/** or app/**
 * A violation is a lint ERROR (fails `npm run lint`, which CI runs), not a review nitpick.
 */
const eslintConfig = defineConfig([
  ...nextVitals,
  ...nextTs,
  {
    plugins: { boundaries },
    settings: {
      "boundaries/include": ["src/**/*"],
      "boundaries/elements": [
        { type: "app", pattern: "src/app/**" },
        { type: "features", pattern: "src/features/*/**", capture: ["feature"] },
        { type: "shared", pattern: "src/shared/**" },
      ],
      "boundaries/ignore": ["**/*.test.ts", "**/*.test.tsx"],
    },
    rules: {
      "boundaries/dependencies": [
        "error",
        {
          default: "disallow",
          policies: [
            {
              from: { element: { type: "app" } },
              allow: { to: { element: { type: ["features", "shared"] } } },
            },
            {
              from: { element: { type: "features" } },
              allow: {
                to: {
                  element: [
                    { type: "shared" },
                    {
                      type: "features",
                      captured: { feature: "{{ from.element.captured.feature }}" },
                    },
                  ],
                },
              },
            },
            // "shared" has no rule here, which is intentional: the "default: disallow" above
            // already blocks shared/** from importing features/** or app/**.

            // Deliberate, narrow exceptions (see IMPLEMENTATION_PLAN.md section 4 and the
            // per-feature build specs): duplicating another feature's list-fetch would be worse
            // than these two explicit, one-directional cross-feature dependencies.
            //   academics -> users:       Teacher/Student pickers for teacher-assignment & enrollment
            //   assignments -> academics: ClassSubject picker when creating an assignment
            {
              from: { element: { type: "features", captured: { feature: "academics" } } },
              allow: { to: { element: { type: "features", captured: { feature: "users" } } } },
            },
            {
              from: { element: { type: "features", captured: { feature: "assignments" } } },
              allow: { to: { element: { type: "features", captured: { feature: "academics" } } } },
            },
          ],
        },
      ],
    },
  },
  globalIgnores([".next/**", "out/**", "build/**", "next-env.d.ts"]),
]);

export default eslintConfig;
