/** Styles inline partages des pages d'auth (avant l'arrivee d'un vrai design system). */
import type { CSSProperties } from 'react';

export const styles: Record<string, CSSProperties> = {
  page: { maxWidth: 420, margin: '8vh auto', padding: 24, fontFamily: 'system-ui, sans-serif' },
  brand: { color: '#1F6F8B', marginBottom: 4 },
  tagline: { fontStyle: 'italic', color: '#666', marginTop: 0 },
  card: { marginTop: 24, padding: 24, borderRadius: 12, border: '1px solid #c9d6dc', background: '#F7FAFB' },
  label: { display: 'block', fontSize: 14, fontWeight: 600, marginBottom: 6, marginTop: 16 },
  input: { width: '100%', padding: '10px 12px', borderRadius: 8, border: '1px solid #c9d6dc', fontSize: 15, boxSizing: 'border-box' },
  checkboxRow: { display: 'flex', gap: 10, alignItems: 'flex-start', marginTop: 18, fontSize: 13, color: '#334' },
  button: { width: '100%', marginTop: 22, padding: '12px 16px', borderRadius: 8, border: 'none', background: '#1F6F8B', color: '#fff', fontSize: 15, fontWeight: 600, cursor: 'pointer' },
  buttonDisabled: { opacity: 0.6, cursor: 'wait' },
  error: { marginTop: 16, padding: 12, borderRadius: 8, background: '#FDF0EF', border: '1px solid #D9534F', color: '#A33B37', fontSize: 14 },
  switchLink: { marginTop: 18, fontSize: 14, textAlign: 'center' as const },
};
