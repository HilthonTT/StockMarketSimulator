import { Ratelimit } from "@upstash/ratelimit";

import { redis } from "@/lib/redis";

// Allow 100 requests per 60 seconds per user/IP (adjust as needed)
export const ratelimit = new Ratelimit({
  redis,
  limiter: Ratelimit.slidingWindow(100, "1m"),
  analytics: true,
  prefix: "@ratelimit",
});
