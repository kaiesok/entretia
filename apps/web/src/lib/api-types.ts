/**
 * Types du contrat d'API.
 *
 * SOURCE DE VERITE : la documentation OpenAPI generee par l'API .NET.
 * Ce fichier sera REGENERE automatiquement par la commande :
 *   pnpm --filter @entretia/web gen:api-types
 * (l'API doit tourner en local — voir README, section "Types partages").
 *
 * En attendant la premiere generation, les types sont declares a la main
 * et doivent rester identiques aux contrats C# (les records *Response.cs
 * dans les dossiers Features de l'API).
 */

export interface HealthResponse {
  status: 'ok';
  service: string;
  timestamp: string;
}
