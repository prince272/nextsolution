/** @type {import('next').NextConfig} */
const nextConfig = {
  // Why my nextjs component is rendering twice?
  // source: https://stackoverflow.com/questions/71847778/why-my-nextjs-component-is-rendering-twice
  reactStrictMode: false,
  env: {
    NEXT_PUBLIC_SERVER_URL: process.env.NEXT_PUBLIC_SERVER_URL
  }
};

export default nextConfig;
