'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { clearAuth, getFirstName, isLoggedIn } from '@/lib/auth';
import { styles } from '@/lib/styles';

export default function DashboardPage() {
  const router = useRouter();
  const [firstName, setFirstName] = useState<string | null>(null);
  const [checked, setChecked] = useState(false);

  // Garde cote client : sans jeton valide, retour a la connexion.
  useEffect(() => {
    if (!isLoggedIn()) {
      router.replace('/login');
      return;
    }
    setFirstName(getFirstName());
    setChecked(true);
  }, [router]);

  if (!checked) {
    return null; // evite un flash de contenu avant la verification
  }

  function handleLogout() {
    clearAuth();
    router.replace('/login');
  }

  return (
    <main style={{ ...styles.page, maxWidth: 640 }}>
      <h1 style={styles.brand}>ENTRETIA</h1>
      <p style={styles.tagline}>Tableau de bord</p>

      <div style={styles.card}>
        <p>
          Bienvenue <strong>{firstName ?? ''}</strong> — ton compte est actif. 🎉
        </p>
        <p style={{ color: '#667', fontSize: 14 }}>
          Prochaine etape (Sprint 2) : depose ton CV pour preparer ton premier
          entretien simule.
        </p>
        <button
          type="button"
          onClick={handleLogout}
          style={{ ...styles.button, background: '#fff', color: '#1F6F8B', border: '1px solid #1F6F8B', width: 'auto', padding: '10px 20px' }}
        >
          Se deconnecter
        </button>
      </div>
    </main>
  );
}
