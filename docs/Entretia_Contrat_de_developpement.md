# Entretia — Contrat de développement

**Version 1.0 — Juillet 2026 — S'applique à tout code mergé sur `main`.**
Ce document fait autorité en cas de désaccord. Toute modification passe par une PR et une ADR.

---

## 1. Architecture : tranches verticales + ports/adapters

### 1.1 La décision

Le code est organisé en **tranches verticales** (Vertical Slice Architecture) : un dossier
par fonctionnalité métier, contenant tout ce qui la concerne.

```
Features/
├── Auth/            (contrôleur, service, contrats, validation)
├── Resumes/
├── Interviews/
│   └── Engine/      (cœur métier : machine à états, orchestration, scoring)
└── Reports/
Ports/               (interfaces vers l'extérieur : ILlmProvider, ISttProvider, ITtsProvider, IFileStorage)
Adapters/            (implémentations : AnthropicLlmProvider, S3FileStorage, ...)
Data/                (EF Core : AppDbContext, migrations)
Common/              (configuration validée, middlewares, filtres d'exceptions)
```

### 1.2 Pourquoi pas la Clean Architecture complète (4 projets) ?

| Critère | Clean Architecture canonique | Notre choix |
|---|---|---|
| Taille d'équipe visée | 5+ développeurs | 1–2 développeurs |
| Coût par fonctionnalité | 4 projets, 3 mappings traversés | 1 dossier |
| Protection du domaine | Par la structure des projets | Par 2 règles simples (ci-dessous) |
| Navigation dans le code | Une fonctionnalité éparpillée en couches | Une fonctionnalité = un dossier |

On garde **l'esprit** de la Clean Architecture avec deux règles au lieu de quatre projets :

- **Règle 1 — Le cœur ne connaît pas l'extérieur.** Le code de `Features/*/Engine` et toute
  logique métier ne référencent JAMAIS un SDK externe (Anthropic, AWS...), uniquement les
  interfaces de `Ports/`. Aucun `using` d'un SDK hors de `Adapters/`.
- **Règle 2 — Les tranches ne se parlent pas directement.** Une feature n'appelle pas le
  service d'une autre feature ; si un besoin transverse apparaît, il est extrait explicitement
  (et discuté en revue).

**Clause d'évolution** : si l'équipe dépasse 3 devs OU si le domaine est consommé par un
second client (ex. app mobile native, worker de fond), on extrait alors un projet
`Entretia.Domain` — la migration est mécanique quand les tranches sont propres.

---

## 2. SOLID — appliqué, avec exemples Entretia

| Principe | Règle pratique dans ce projet |
|---|---|
| **S** — Responsabilité unique | Un service = une raison de changer. `InterviewEngine` conduit l'entretien ; `ScoringService` évalue ; `ReportBuilder` met en forme. Si un fichier dépasse ~300 lignes, on questionne son découpage. |
| **O** — Ouvert/fermé | Ajouter un style de recruteur (bienveillant/exigeant) ou un type d'entretien = ajouter une stratégie (nouvelle classe + enregistrement DI), pas modifier des `switch` existants. |
| **L** — Substitution de Liskov | Toute implémentation de `ILlmProvider` est interchangeable sans casser l'appelant : mêmes garanties (timeout, exceptions typées), pas de comportement surprise. |
| **I** — Ségrégation des interfaces | Des petits ports : `ILlmProvider`, `ISttProvider`, `ITtsProvider`, `IFileStorage` — jamais un fourre-tout `IAiService`. Un consommateur ne dépend que de ce qu'il utilise. |
| **D** — Inversion de dépendance | Le moteur dépend d'abstractions, les SDK sont injectés. C'est ce qui rend le cœur testable sans API payante et le changement de fournisseur indolore. |

SOLID est une boussole, pas un dogme : on ne crée pas une interface pour une classe qui n'aura
jamais de seconde implémentation ni de besoin de mock (YAGNI). Le critère : *testabilité* ou
*point de variation identifié* — sinon, classe concrète.

## 3. Catalogue des patterns retenus (et écartés)

**Retenus :**
- **Ports & Adapters** : tout service externe (LLM, STT, TTS, stockage) derrière une interface.
- **Strategy** : styles de recruteur, types d'entretien.
- **Machine à états explicite** : cycle de vie des sessions (voir ADR 002) ; les transitions
  invalides sont impossibles par construction.
- **Options pattern + ValidateOnStart** : configuration typée et validée au démarrage (fail fast).
- **Records C# pour les contrats** : immuables, comparables, exposés dans OpenAPI.

**Écartés (délibérément, pour le MVP) :**
- **Repository générique au-dessus d'EF Core** : `DbContext` EST déjà un unit-of-work +
  repository ; l'emballer ajoute de l'indirection sans valeur. Les requêtes complexes vont
  dans des méthodes d'extension ou des services de requête dédiés.
- **MediatR / CQRS complet** : de la cérémonie sans bénéfice à notre échelle. Les contrôleurs
  appellent les services directement. Réévaluable en clause d'évolution.
- **Microservices** : monolithe modulaire assumé. Les tranches verticales rendent une
  extraction future possible si (et seulement si) un besoin réel apparaît.

---

## 4. Tests et TDD — politique différenciée

### 4.1 Où le TDD est OBLIGATOIRE (rouge → vert → refactor)

Le **cœur métier**, car il est pur (pas d'E/S), critique, et le TDD y améliore réellement le design :
- Machine à états de session (transitions valides/invalides, reprise après interruption).
- Logique du moteur d'entretien (choix de la phase suivante, conditions de relance, budget).
- Calcul des scores et agrégation du rapport.

### 4.2 Où les tests viennent APRÈS le code (mais restent exigés)

- Contrôleurs et validation d'entrée : tests ciblés sur les comportements (codes HTTP, rejets).
- Adapters : tests d'intégration (base PostgreSQL éphémère via Testcontainers à partir du Sprint 1).
- Frontend : tests des composants critiques uniquement (salle d'entretien) à partir du Sprint 3.

### 4.3 Règles absolues

1. **Aucun test n'appelle une API IA réelle.** Les ports sont mockés. Un test qui dépend
   d'Anthropic est lent, coûteux et non déterministe : refusé en revue.
2. **Tout bug corrigé commence par un test qui le reproduit** (rouge), puis le correctif (vert).
   Le bug ne peut plus revenir silencieusement.
3. **La couverture n'est pas un objectif chiffré** ; la cible qualitative est : tout le cœur
   métier testé, tous les cas d'erreur des contrôleurs testés. Un test qui ne peut pas échouer
   (test du framework, assertions triviales) est supprimé.
4. La pyramide est respectée : beaucoup de tests unitaires rapides, quelques tests
   d'intégration, très peu de tests bout-en-bout.

---

## 5. Intelligence artificielle — deux périmètres distincts

### 5.1 L'IA DANS le produit : un LLM orchestré, PAS un agent autonome

Le recruteur virtuel n'est pas un agent libre de ses actions. L'architecture est :

```
Machine à états (NOTRE code, déterministe, testé en TDD)
    └── décide : la phase, quand relancer, quand conclure, le budget restant
         └── LLM (appelé via ILlmProvider)
              └── décide : le CONTENU de la prochaine question / l'évaluation
                   └── sortie STRUCTURÉE et VALIDÉE (schéma JSON) avant usage
```

Justification :
- **Prévisibilité** : face à un candidat, un comportement borné est une exigence produit.
- **Testabilité** : le flux de contrôle est du code ordinaire, testable sans LLM.
- **Coûts bornés** : le nombre d'appels par session est structurellement plafonné.
- **Sécurité** : toute sortie LLM est traitée comme une entrée non fiable — validée
  (schéma), jamais exécutée, jamais insérée telle quelle dans une requête.

Si un besoin réellement agentique apparaît (ex. le recruteur consulte des outils), il fera
l'objet d'une ADR dédiée avec analyse de risques.

### 5.2 L'IA POUR développer : oui, sous contrat

Les assistants (Claude, Claude Code, Copilot...) sont encouragés pour : squelettes de code,
tests, refactoring, revue préliminaire, documentation, exploration d'API.

Règles non négociables :
1. **Même circuit qualité** : le code assisté par IA passe par la même PR, la même CI et une
   relecture humaine. « L'IA l'a écrit » n'est jamais une justification en revue.
2. **Confidentialité** : aucun secret (clés, mots de passe), aucune donnée candidat réelle
   (CV, transcriptions) n'est collé dans un prompt. Les exemples utilisent des données fictives.
3. **Les décisions restent humaines** : architecture, sécurité, arbitrages produit — l'IA
   propose, un humain décide, l'ADR trace.
4. **Comprendre avant de merger** : on ne merge pas du code qu'on ne saurait pas expliquer
   ligne par ligne en revue.
5. Les prompts du produit (system prompt du recruteur, grilles d'évaluation) sont **versionnés
   dans le dépôt** comme du code, avec revue — ce sont nos actifs les plus précieux.

---

## 6. Conventions transverses (rappel)

- **Git** : branches `feat/xxx` / `fix/xxx`, Conventional Commits, merge sur `main` par PR
  uniquement, CI verte obligatoire, relecture obligatoire.
- **C#** : `TreatWarningsAsErrors` activé ; nullable activé ; pas de `#pragma warning disable`
  sans commentaire justificatif.
- **Secrets** : user-secrets en dev, variables d'environnement en prod, jamais dans Git.
- **Erreurs** : exceptions typées côté domaine, filtre global côté API, jamais de stack trace
  exposée au client.
- **Definition of Done** : code relu et mergé · tests verts en CI · entrées validées · erreurs
  gérées · OpenAPI à jour · démontré en staging.

---

## 7. Ce que ce contrat interdit explicitement

- Référencer un SDK externe hors de `Adapters/`.
- Un test qui appelle une API payante.
- Un merge sans relecture humaine, quelle que soit l'origine du code.
- Un secret ou une donnée candidat dans un prompt, un log ou un commit.
- Modifier le schéma de base sans migration versionnée.
- Une décision d'architecture non tracée par ADR.
