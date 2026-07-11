/**
 * Stockage du jeton cote navigateur.
 *
 * Choix MVP assume : localStorage. Simple et suffisant pour demarrer,
 * mais lisible par tout script s'executant sur la page (risque XSS).
 * Evolution prevue (ADR a venir) : cookie httpOnly pose par l'API.
 * Toute la logique passe par ce module : le jour du changement,
 * un seul fichier bouge.
 */

import type { AuthResponse } from './api-types';

const TOKEN_KEY = 'entretia.accessToken';
const EMAIL_KEY = 'entretia.email';
const FIRSTNAME_KEY = 'entretia.firstName';
const EXPIRES_KEY = 'entretia.expiresAt';

export function saveAuth(auth: AuthResponse): void {
  localStorage.setItem(TOKEN_KEY, auth.accessToken);
  localStorage.setItem(EMAIL_KEY, auth.email);
  localStorage.setItem(FIRSTNAME_KEY, auth.firstName);
  localStorage.setItem(EXPIRES_KEY, auth.expiresAt);
}

export function getToken(): string | null {
  if (typeof window === 'undefined') return null; // rendu cote serveur

  const expiresAt = localStorage.getItem(EXPIRES_KEY);
  if (expiresAt && new Date(expiresAt) <= new Date()) {
    clearAuth(); // jeton expire : on nettoie
    return null;
  }
  return localStorage.getItem(TOKEN_KEY);
}

export function getEmail(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem(EMAIL_KEY);
}

export function getFirstName(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem(FIRSTNAME_KEY);
}

export function isLoggedIn(): boolean {
  return getToken() !== null;
}

export function clearAuth(): void {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(EMAIL_KEY);
  localStorage.removeItem(FIRSTNAME_KEY);
  localStorage.removeItem(EXPIRES_KEY);
}
