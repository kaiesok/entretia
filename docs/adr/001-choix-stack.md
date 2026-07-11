# ADR 001 — Choix de la stack : TypeScript full-stack (Next.js + NestJS)

**Date : juillet 2026 — Statut : REMPLACE par ADR 003**

## Contexte
Entretia repose sur une boucle conversationnelle temps reel (audio -> STT -> LLM -> TTS -> avatar)
et une petite equipe de developpement. Options evaluees : .NET 8, Node.js/NestJS, Python/FastAPI.

## Decision
TypeScript de bout en bout : Next.js 14 (frontend) + NestJS (backend) + PostgreSQL/Prisma.

## Justification
- Un seul langage : types partages via `@entretia/shared`, pas de duplication des contrats.
- Temps reel natif (WebSocket, streaming) bien outille dans l'ecosysteme Node.
- SDK IA (Anthropic, Deepgram, ElevenLabs) TypeScript-first avec streaming.
- NestJS structure le code (modules, DI) et son modele est familier aux developpeurs .NET.

## Consequences
- Toute structure echangee front/back est definie dans `packages/shared`.
- Reevaluation possible si l'equipe devient majoritairement .NET (voir plan de dev).
