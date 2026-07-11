'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { apiPost } from '@/lib/api-client';
import { saveAuth } from '@/lib/auth';
import { styles } from '@/lib/styles';
import type { AuthResponse, LoginRequest } from '@/lib/api-types';

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    setError(null);
    setSubmitting(true);

    const body: LoginRequest = { email, password };
    const result = await apiPost<AuthResponse>('/api/auth/login', body);
    setSubmitting(false);

    if (!result.ok) {
      setError(result.error);
      return;
    }

    saveAuth(result.data);
    router.push('/dashboard');
  }

  return (
    <main style={styles.page}>
      <h1 style={styles.brand}>ENTRETIA</h1>
      <p style={styles.tagline}>Connexion</p>

      <form style={styles.card} onSubmit={handleSubmit}>
        <label style={styles.label} htmlFor="email">Email</label>
        <input
          id="email"
          type="email"
          required
          autoComplete="email"
          style={styles.input}
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />

        <label style={styles.label} htmlFor="password">Mot de passe</label>
        <input
          id="password"
          type="password"
          required
          autoComplete="current-password"
          style={styles.input}
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />

        {error && <div style={styles.error} role="alert">{error}</div>}

        <button
          type="submit"
          disabled={submitting}
          style={{ ...styles.button, ...(submitting ? styles.buttonDisabled : {}) }}
        >
          {submitting ? 'Connexion…' : 'Se connecter'}
        </button>

        <p style={styles.switchLink}>
          Pas encore de compte ? <Link href="/register">Creer un compte</Link>
        </p>
      </form>
    </main>
  );
}
