/**
 * Types du contrat d'API.
 *
 * SOURCE DE VERITE : la documentation OpenAPI generee par l'API .NET
 * (http://localhost:3001/swagger). Regeneration automatique :
 *   pnpm --filter @entretia/web gen:api-types
 * En attendant, miroir manuel des records C# (Features/Auth/AuthContracts.cs).
 */

export interface HealthResponse {
  status: 'ok';
  service: string;
  timestamp: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phone: string | null;
  acceptDataProcessing: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  accessToken: string;
  expiresAt: string;
}
