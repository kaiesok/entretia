/** @type {import('next').NextConfig} */
const nextConfig = {
  transpilePackages: ['@entretia/shared'],
  // Les appels /api/* sont relayes vers le backend NestJS :
  // le navigateur ne connait qu'une seule origine (pas de CORS en dev).
  async rewrites() {
    return [
      {
        source: '/api/:path*',
        destination: `${process.env.API_URL ?? 'http://localhost:3001'}/api/:path*`,
      },
    ];
  },
};

export default nextConfig;
