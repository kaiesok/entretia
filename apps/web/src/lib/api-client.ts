/**
 * Client HTTP unique de l'application.
 * - Toutes les requetes passent par ici : gestion d'erreurs centralisee.
 * - Les appels partent vers /api/... : la reecriture de next.config.mjs
 *   les relaie vers l'API .NET (une seule origine vue du navigateur).
 */

import { getToken } from './auth';

export type ApiResult<T> =
  | { ok: true; data: T }
  | { ok: false; error: string; status: number };

/** Extrait un message lisible des differents formats d'erreur de l'API. */
function extractError(body: unknown, status: number): string {
  if (body && typeof body === 'object') {
    // Nos erreurs metier : { error: "..." }
    const withError = body as { error?: unknown };
    if (typeof withError.error === 'string') return withError.error;

    // Les erreurs de validation ASP.NET : { errors: { Champ: ["msg"] } }
    const withErrors = body as { errors?: Record<string, string[]> };
    if (withErrors.errors) {
      const first = Object.values(withErrors.errors).flat()[0];
      if (first) return first;
    }
  }
  return `Erreur inattendue (HTTP ${status}). Reessaie ou contacte le support.`;
}

export async function apiPost<TResponse>(
  path: string,
  body: unknown,
): Promise<ApiResult<TResponse>> {
  try {
    const token = getToken();
    const res = await fetch(path, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
      body: JSON.stringify(body),
    });

    const responseBody: unknown = await res.json().catch(() => null);

    if (!res.ok) {
      return { ok: false, error: extractError(responseBody, res.status), status: res.status };
    }

    return { ok: true, data: responseBody as TResponse };
  } catch {
    return {
      ok: false,
      error: "Impossible de joindre le serveur. Verifie ta connexion.",
      status: 0,
    };
  }
}
