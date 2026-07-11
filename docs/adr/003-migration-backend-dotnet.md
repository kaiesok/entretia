# ADR 003 — Migration du backend vers .NET 8 (remplace ADR 001)

**Date : juillet 2026 — Statut : accepte — Remplace : ADR 001**

## Contexte
L'ADR 001 avait retenu NestJS pour le backend. La decision est revue :
l'equipe de developpement pressentie est experimentee en C#/.NET, ce qui etait
precisement la condition de reevaluation notee dans l'ADR 001 et le plan de dev.

## Decision
Backend en ASP.NET Core 8 (Web API + controllers), EF Core + Npgsql pour
PostgreSQL, xUnit pour les tests. Le frontend reste Next.js.

## Ce qui remplace quoi
| Avant (NestJS)              | Apres (.NET 8)                          |
|-----------------------------|------------------------------------------|
| Modules NestJS              | Feature folders (Features/Auth, ...)     |
| class-validator             | DataAnnotations + FluentValidation (S1)  |
| Prisma                      | EF Core + migrations                     |
| Zod validateEnv()           | Options pattern + ValidateOnStart        |
| package @entretia/shared    | Types TS generes depuis OpenAPI          |
| Socket.io                   | SignalR (Sprint 3/5)                     |

## Consequences
- Le contrat front/back a une source de verite unique : la doc OpenAPI de
  l'API. Le frontend regenere ses types avec `pnpm gen:api-types`.
- Les secrets de developpement passent par `dotnet user-secrets`,
  ceux de production par variables d'environnement.
- SignalR remplacera Socket.io pour le temps reel : natif, performant,
  et bien documente cote .NET.
