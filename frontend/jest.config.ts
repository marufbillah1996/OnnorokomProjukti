import type { Config } from "jest";
import nextJest from "next/jest";

// next/jest is the officially documented way to wire Jest into a Next.js app: it loads
// next.config.ts/babel config for us and applies the same SWC transforms Next.js itself uses,
// so TSX/CSS/module imports inside src/** behave the same under test as they do in the app.
const createJestConfig = nextJest({ dir: "./" });

const customJestConfig: Config = {
  // Runs once per test file, after the test framework (jest-dom matchers etc.) is installed in
  // the environment but before the test file's own code runs. NOTE: the correct Jest option
  // name here is "setupFilesAfterEnv" (verified against this repo's installed jest-config /
  // @jest/types source — see node_modules/@jest/types/build/index.d.ts), not any "AfterEach"
  // variant.
  setupFilesAfterEnv: ["<rootDir>/jest.setup.ts"],
  testEnvironment: "jest-environment-jsdom",
  moduleNameMapper: {
    "^@/(.*)$": "<rootDir>/src/$1",
  },
  testPathIgnorePatterns: ["<rootDir>/node_modules/", "<rootDir>/.next/"],
};

export default createJestConfig(customJestConfig);
