import { defineConfig, globalIgnores } from "eslint/config";
import nextVitals from "eslint-config-next/core-web-vitals";
import nextTs from "eslint-config-next/typescript";

// eslint-config-next 16 ships native flat configs (no FlatCompat shim needed).
export default defineConfig([
  ...nextVitals,
  ...nextTs,
  {
    // New React Compiler advisory rules (eslint-config-next 16). They flag existing prop->state
    // sync patterns and generated shadcn components; keep them visible as warnings for now.
    rules: {
      "react-hooks/set-state-in-effect": "warn",
      "react-hooks/purity": "warn",
    },
  },
  globalIgnores([".next/**", "out/**", "build/**", "next-env.d.ts"]),
]);
