# Entretia

Plateforme de simulation d'entretiens d'embauche pilotee par IA — avatar recruteur,
questions adaptatives selon le CV et les reponses du candidat, rapport d'evaluation.

Stack : **Next.js 14 (frontend) · ASP.NET Core 8 / C# (backend) · PostgreSQL (EF Core)**

## Prerequis

- **SDK .NET 8** — https://dotnet.microsoft.com/download/dotnet/8.0 (`dotnet --version` -> 8.0.x)
- **Node.js >= 20** + **pnpm 9** (`corepack enable` puis `corepack prepare pnpm@9 --activate`)
- **Docker Desktop** (PostgreSQL et MinIO en local)

## Demarrage (5 minutes)

```bash
# 1. Cloner et installer le frontend
git clone <url-du-depot> && cd entretia
pnpm install
cp .env.example .env

# 2. Lancer la base de donnees et le stockage
docker compose up -d

# 3. Terminal 1 — API .NET (port 3001)
pnpm dev:api
#    Premiere fois : dotnet restaure les packages NuGet automatiquement.

# 4. Terminal 2 — Frontend (port 3000)
pnpm dev:web
```

- Frontend : http://localhost:3000 (doit afficher « API connectee »)
- API : http://localhost:3001/api/health
- Documentation interactive de l'API : http://localhost:3001/swagger
- Console MinIO : http://localhost:9001 (entretia / entretia_dev)

Tests backend : `pnpm test:api` (ou `dotnet test apps/api/Entretia.sln`).

## Structure

```
apps/web                    Next.js — interface candidat
apps/api/src/Entretia.Api   ASP.NET Core — API, SignalR (a venir), orchestration IA
  ├── Features/             Un dossier par domaine metier (Health, puis Auth, Resumes...)
  ├── Data/                 EF Core (AppDbContext, migrations)
  └── Common/               Configuration validee, filtres, middlewares
apps/api/tests              xUnit
docs/adr                    Decisions d'architecture (une par decision structurante)
```

## Types partages front/back

La **source de verite du contrat d'API est la documentation OpenAPI** generee
par le backend. Apres tout changement de contrat cote C#, regenerer les types
TypeScript du frontend (l'API doit tourner) :

```bash
pnpm gen:api-types
```

Le fichier `apps/web/src/lib/api-types.ts` est ecrase — ne jamais l'editer a la main
apres la premiere generation.

## Secrets

- **Jamais de secret dans Git** (ni dans appsettings.json).
- Developpement : `dotnet user-secrets set "Cle" "valeur" --project apps/api/src/Entretia.Api`
  (exemple au Sprint 1 : `dotnet user-secrets set "Jwt:Secret" "..."`).
- Production : variables d'environnement (`ConnectionStrings__Default`, `Jwt__Secret`, ...).

## Regles de contribution

- Branches : `feat/xxx`, `fix/xxx` — merge sur `main` par pull request uniquement.
- Commits : Conventional Commits (`feat:`, `fix:`, `test:`, `docs:`, `chore:`).
- Chaque PR doit passer la CI (lint + build + tests) ; les avertissements
  du compilateur C# sont traites comme des erreurs.
- Toute entree utilisateur est validee cote API ; toute sortie LLM est validee.

## Feuille de route

Voir `docs/Entretia_Plan_de_developpement.md` — 7 sprints :
fondations -> auth -> CV -> moteur d'entretien texte -> rapport -> voix -> avatar.
