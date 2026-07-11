'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { apiPost } from '@/lib/api-client';
import { saveAuth } from '@/lib/auth';
import { styles } from '@/lib/styles';
import type { AuthResponse, RegisterRequest } from '@/lib/api-types';

const PASSWORD_MIN_LENGTH = 12; // aligne sur PasswordPolicy cote API

export default function RegisterPage() {
  const router = useRouter();
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [password, setPassword] = useState('');
  const [consent, setConsent] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    setError(null);

    // Pre-validation cote client : confort utilisateur uniquement.
    // La SEULE validation qui fait foi est celle de l'API.
    if (firstName.trim().length === 0 || lastName.trim().length === 0) {
      setError('Le prenom et le nom sont obligatoires.');
      return;
    }
    if (password.length < PASSWORD_MIN_LENGTH) {
      setError(`Le mot de passe doit contenir au moins ${PASSWORD_MIN_LENGTH} caracteres. Astuce : une phrase entiere fonctionne tres bien.`);
      return;
    }
    if (!consent) {
      setError('Le consentement au traitement des donnees est necessaire pour creer un compte.');
      return;
    }

    setSubmitting(true);
    const body: RegisterRequest = {
      email,
      password,
      firstName: firstName.trim(),
      lastName: lastName.trim(),
      phone: phone.trim() === '' ? null : phone.trim(),
      acceptDataProcessing: consent,
    };
    const result = await apiPost<AuthResponse>('/api/auth/register', body);
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
      <p style={styles.tagline}>Creer votre compte</p>

      <form style={styles.card} onSubmit={handleSubmit}>
        <div style={{ display: 'flex', gap: 12 }}>
          <div style={{ flex: 1 }}>
            <label style={styles.label} htmlFor="firstName">Prenom *</label>
            <input
              id="firstName"
              type="text"
              required
              autoComplete="given-name"
              style={styles.input}
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
            />
          </div>
          <div style={{ flex: 1 }}>
            <label style={styles.label} htmlFor="lastName">Nom *</label>
            <input
              id="lastName"
              type="text"
              required
              autoComplete="family-name"
              style={styles.input}
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
            />
          </div>
        </div>

        <label style={styles.label} htmlFor="email">Email *</label>
        <input
          id="email"
          type="email"
          required
          autoComplete="email"
          style={styles.input}
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />

        <label style={styles.label} htmlFor="phone">Telephone (optionnel)</label>
        <input
          id="phone"
          type="tel"
          autoComplete="tel"
          placeholder="+216 20 123 456"
          style={styles.input}
          value={phone}
          onChange={(e) => setPhone(e.target.value)}
        />

        <label style={styles.label} htmlFor="password">Mot de passe *</label>
        <input
          id="password"
          type="password"
          required
          autoComplete="new-password"
          minLength={PASSWORD_MIN_LENGTH}
          style={styles.input}
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />

        <div style={styles.checkboxRow}>
          <input
            id="consent"
            type="checkbox"
            checked={consent}
            onChange={(e) => setConsent(e.target.checked)}
            style={{ marginTop: 2 }}
          />
          <label htmlFor="consent">
            J&apos;accepte que mes donnees (identite, email, CV, transcriptions d&apos;entretien)
            soient traitees par Entretia pour fournir le service. Je peux retirer ce
            consentement et supprimer mon compte a tout moment.
          </label>
        </div>

        {error && <div style={styles.error} role="alert">{error}</div>}

        <button
          type="submit"
          disabled={submitting}
          style={{ ...styles.button, ...(submitting ? styles.buttonDisabled : {}) }}
        >
          {submitting ? 'Creation du compte…' : 'Creer mon compte'}
        </button>

        <p style={styles.switchLink}>
          Deja un compte ? <Link href="/login">Se connecter</Link>
        </p>
      </form>
    </main>
  );
}
