import { useSyncExternalStore } from "react";

const subscribe = () => () => {};

/**
 * `false` during SSR and hydration, `true` afterwards. Shell components read queries that a page may
 * prefetch and stream in later; rendering their data only after hydration keeps the server HTML
 * and the first client render identical (no hydration mismatch).
 */
export function useHydrated() {
  return useSyncExternalStore(
    subscribe,
    () => true,
    () => false
  );
}
