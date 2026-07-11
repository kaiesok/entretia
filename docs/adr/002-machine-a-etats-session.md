# ADR 002 — Sessions d'entretien modelisees en machine a etats persistee

**Date : juillet 2026 — Statut : accepte**

## Contexte
Une session d'entretien est un processus long (15-45 min), interruptible (coupure reseau,
fermeture du navigateur), avec des phases distinctes.

## Decision
Chaque session suit une machine a etats explicite, persistee en base a chaque transition :
CREATED -> INTRO -> QUESTIONING <-> FOLLOW_UP -> CANDIDATE_QUESTIONS -> CLOSING -> REPORT_READY

## Consequences
- Une session interrompue reprend la ou elle s'etait arretee.
- Les transitions invalides sont refusees par le code (pas d'etat incoherent).
- L'enum de reference vit dans `@entretia/shared` (InterviewState).
