import type { HealthResponse } from '@/lib/api-types';

/**
 * Page d'accueil provisoire (Sprint 0) : verifie que le frontend
 * communique avec l'API. Sera remplacee par le tableau de bord au Sprint 1.
 */
async function getApiHealth(): Promise<HealthResponse | null> {
  try {
    const apiUrl = process.env.API_URL ?? 'http://localhost:3001';
    const res = await fetch(`${apiUrl}/api/health`, { cache: 'no-store' });
    if (!res.ok) return null;
    return (await res.json()) as HealthResponse;
  } catch {
    return null;
  }
}

export default async function HomePage() {
  const health = await getApiHealth();

  return (
    <main style={{ maxWidth: 640, margin: '10vh auto', padding: 24 }}>
      <h1 style={{ color: '#1F6F8B' }}>ENTRETIA</h1>
      <p style={{ fontStyle: 'italic', color: '#666' }}>
        Votre entretien, avant l&apos;entretien.
      </p>
      <div
        style={{
          marginTop: 32,
          padding: 16,
          borderRadius: 12,
          border: '1px solid #c9d6dc',
          background: health ? '#EFF7EF' : '#FDF0EF',
        }}
      >
        {health ? (
          <p>
            API connectee — service <strong>{health.service}</strong> ({health.timestamp})
          </p>
        ) : (
          <p>
            API injoignable. Verifier que l&apos;API .NET tourne (<code>dotnet watch</code>)
            et que le port 3001 est libre.
          </p>
        )}
      </div>
    </main>
  );
}
