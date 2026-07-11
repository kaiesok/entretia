import type { Metadata } from 'next';
import type { ReactNode } from 'react';

export const metadata: Metadata = {
  title: 'Entretia — Votre entretien, avant l\'entretien',
  description: 'Simulation d\'entretiens d\'embauche pilotee par IA',
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="fr">
      <body style={{ fontFamily: 'system-ui, sans-serif', margin: 0 }}>{children}</body>
    </html>
  );
}
