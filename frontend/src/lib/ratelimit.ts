import { Ratelimit } from "@upstash/ratelimit";

import { redis } from "@/lib/redis";

type Limiter = Pick<Ratelimit, "limit">;

/**
 * Allows 100 requests per 60 seconds per user. Upstash is optional for local development:
 * without credentials every request is allowed (previously every protected call failed).
 */
export const ratelimit: Limiter = redis
  ? new Ratelimit({
      redis,
      limiter: Ratelimit.slidingWindow(100, "1m"),
      analytics: true,
      prefix: "@ratelimit",
    })
  : {
      limit: async () =>
        ({
          success: true,
          limit: Infinity,
          remaining: Infinity,
          reset: 0,
          pending: Promise.resolve(),
        }) as Awaited<ReturnType<Ratelimit["limit"]>>,
    };
