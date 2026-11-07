import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Self-contained server bundle for the Docker image (see Dockerfile).
  output: "standalone",
  images: {
    remotePatterns: [
      {
        protocol: "https",
        hostname: "localhost",
      },
    ],
  },
  async redirects() {
    return [
      // The old contact form never sent anything; feedback now goes through GitHub (see /about).
      { source: "/contact", destination: "/about", permanent: true },
    ];
  },
};

export default nextConfig;
