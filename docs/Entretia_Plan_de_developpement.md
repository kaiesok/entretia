# Entretia — Plan de développement détaillé (MVP)

**Version 1.0 — Juillet 2026**
Stack retenue : Next.js 14 (frontend) · NestJS (backend) · PostgreSQL · TypeScript partout

---

## 1. Principes directeurs

Le développement suit quatre règles non négociables, appliquées dès le premier commit :

1. **Tranches verticales** : chaque sprint livre une fonctionnalité complète et démontrable de bout en bout (UI → API → base de données), jamais « tout le backend d'abord ».
2. **Le moteur d'entretien avant le spectacle** : on valide la qualité de l'entretien adaptatif en mode texte avant d'investir dans la voix et l'avatar. C'est le cœur de la valeur produit.
3. **Coûts IA maîtrisés dès la conception** : chaque appel LLM/STT/TTS est loggé avec son coût estimé ; un budget par session est défini et surveillé.
4. **Sécurité et RGPD intégrés, pas rajoutés** : hachage des mots de passe, consentement explicite, chiffrement — dès le sprint concerné, pas « en phase 2 ».

---

## 2. Organisation du projet

### 2.1 Structure (monorepo)

```
entretia/
├── apps/
│   ├── web/                  # Next.js — interface candidat
│   └── api/                  # NestJS — API + WebSocket
├── packages/
│   ├── shared/               # Types TypeScript partagés (DTO, enums)
│   └── config/               # ESLint, TSConfig, Prettier partagés
├── docs/                     # ADR (décisions d'architecture), API, schémas
├── docker-compose.yml        # PostgreSQL + MinIO en local
└── .github/workflows/        # CI : lint + tests + build
```

Outillage : **pnpm workspaces** (ou Turborepo), **Docker Compose** pour l'environnement local identique pour tous.

### 2.2 Conventions Git

| Règle | Détail |
|---|---|
| Branches | `main` (protégée) · `feat/xxx` · `fix/xxx` — merge par pull request uniquement |
| Commits | Conventional Commits : `feat:`, `fix:`, `chore:`, `docs:`, `test:` |
| Revue | Aucun merge sans relecture (Claude peut jouer ce rôle sur chaque PR) |
| CI | Chaque PR : lint + tests + build doivent passer |

### 2.3 Environnements et secrets

- `.env.local` (jamais commité), `.env.example` commité comme référence.
- Trois environnements : `local` → `staging` → `production`.
- Clés API (LLM, TTS, STT) uniquement côté serveur — **jamais** exposées au frontend.
- Validation de la config au démarrage (le serveur refuse de démarrer si une variable manque).

---

## 3. Architecture backend (NestJS)

Modules découplés, chacun avec ses controllers / services / tests :

```
api/src/
├── auth/            # Inscription, connexion, JWT, refresh tokens
├── users/           # Profils, préférences
├── resumes/         # Upload CV, extraction texte, analyse LLM
├── interviews/      # Sessions : machine à états de l'entretien
│   ├── engine/      # Cœur : prompts, contexte, adaptation des questions
│   └── gateway/     # WebSocket temps réel
├── reports/         # Génération et stockage des rapports
├── ai/              # Adaptateurs LLM / STT / TTS (interface commune)
└── common/          # Guards, filters, interceptors, logging
```

**Décision clé — le module `ai/` en adaptateurs** : chaque fournisseur (Anthropic, Deepgram, ElevenLabs…) est derrière une interface (`LlmProvider`, `SttProvider`, `TtsProvider`). On peut changer de fournisseur sans toucher au reste du code, et on peut les *mocker* dans les tests.

**Machine à états d'une session d'entretien** :

```
CREATED → INTRO → QUESTIONING ⇄ FOLLOW_UP → CANDIDATE_QUESTIONS → CLOSING → REPORT_READY
```

Chaque transition est persistée : une session interrompue (coupure réseau) peut reprendre.

---

## 4. Modèle de données (PostgreSQL + Prisma)

| Table | Champs principaux |
|---|---|
| `users` | id, email, password_hash (argon2), locale, created_at |
| `resumes` | id, user_id, file_url, extracted_text, analysis_json (compétences, expériences, points à creuser) |
| `interview_sessions` | id, user_id, resume_id, job_description, type, difficulty, language, state, started_at, ended_at, cost_cents |
| `interview_turns` | id, session_id, turn_number, role (interviewer/candidate), text, audio_url, latency_ms, created_at |
| `reports` | id, session_id, global_score, scores_json (par compétence), strengths_json, improvements_json, annotated_transcript_json |
| `consents` | id, user_id, type (audio/video/data), granted_at, revoked_at |

Migrations versionnées avec Prisma Migrate ; jamais de modification manuelle du schéma.

---

## 5. Phasage — 7 sprints de 2 semaines (~14 semaines)

### Sprint 0 — Fondations (1 semaine, hors compte)

- Monorepo, Docker Compose (PostgreSQL + MinIO), CI GitHub Actions.
- ESLint + Prettier + Husky (hooks pre-commit).
- Squelettes Next.js et NestJS qui communiquent (`/health`).
- **Livrable : `git clone` → `docker compose up` → app qui tourne en local.**

### Sprint 1 — Authentification et comptes

- Inscription / connexion (email + mot de passe, argon2, JWT + refresh token).
- Pages login/signup, layout général, garde d'authentification.
- Table `consents` et écran de consentement RGPD.
- Tests : unitaires (service auth) + e2e (parcours inscription).
- **Livrable : un utilisateur crée un compte et accède à son espace vide.**

### Sprint 2 — Upload et analyse du CV

- Upload PDF (validation type/taille), stockage MinIO/S3, extraction texte.
- Analyse LLM du CV → JSON structuré : compétences, expériences, incohérences ou zones à creuser (avec schéma de sortie validé par Zod).
- UI : dépôt de CV, affichage de l'analyse « Ce que le recruteur verra ».
- **Livrable : CV déposé → fiche d'analyse visible. Première valeur utilisateur réelle.**

### Sprint 3 — Moteur d'entretien en mode TEXTE (le sprint le plus important)

- Machine à états de session ; configuration (type, durée, difficulté, style du recruteur).
- Ingénierie des prompts : system prompt du recruteur avec CV + offre + grille d'évaluation + historique ; règles d'adaptation (relance si réponse vague, approfondissement, question piège).
- Interface de chat d'entretien (texte) avec WebSocket et streaming des réponses.
- Journal des coûts par session.
- **Livrable : un entretien adaptatif complet au clavier. C'est ici qu'on itère le plus sur la qualité des questions — prévoir 15 à 20 entretiens de test.**

### Sprint 4 — Rapport d'évaluation

- Génération du rapport en fin de session : score global, scores par compétence, points forts / axes d'amélioration, transcription annotée (LLM en mode évaluateur, distinct du mode recruteur).
- UI rapport (jauge, radar, verbatims commentés) + historique des sessions.
- **Livrable : le parcours MVP texte est complet : CV → entretien → rapport.**

### Sprint 5 — La voix

- STT : transcription en direct de la parole du candidat (Deepgram ou Whisper API), avec détection de fin de parole.
- TTS : voix naturelle du recruteur (ElevenLabs ou Azure), streaming audio.
- UI : indicateurs micro/écoute, sous-titres en direct, mode texte conservé en accessibilité.
- Mesure de latence bout-en-bout ; cible < 2 s (question suivante après fin de réponse).
- **Livrable : entretien entièrement vocal.**

### Sprint 6 — Avatar animé + pilote

- Avatar 2D/3D avec lip-sync piloté par l'audio TTS (solution simple d'abord : avatar animé, pas photoréaliste).
- Écran « salle d'entretien » final (conforme à la maquette).
- Pilote : 20–50 candidats réels, grille de feedback, corrections.
- **Livrable : MVP démontrable au client, testé par de vrais utilisateurs.**

---

## 6. Qualité — règles permanentes

| Domaine | Pratique |
|---|---|
| Tests | Unitaires sur toute logique métier (moteur, scoring) ; e2e sur les parcours critiques ; les adaptateurs IA sont mockés — aucun test ne dépend d'une API externe |
| Validation | Toute entrée validée (class-validator côté API, Zod pour les sorties LLM) — ne jamais faire confiance ni au client ni au LLM |
| Erreurs | Filtre d'exceptions global, messages utilisateurs clairs, jamais de stack trace exposée |
| Logs | Logging structuré (pino) avec request-id ; log de chaque appel IA : fournisseur, tokens, latence, coût |
| Sécurité | Argon2, rate limiting sur l'auth, CORS strict, en-têtes de sécurité (helmet), dépendances auditées en CI |
| RGPD | Consentement avant toute captation ; suppression de compte = purge effective des CV et enregistrements ; données hébergées en Europe |
| Documentation | OpenAPI auto-générée (Swagger) ; une ADR (Architecture Decision Record) par décision structurante dans `/docs` |

---

## 7. Coûts variables à surveiller (ordre de grandeur par session de 30 min)

| Service | Estimation |
|---|---|
| LLM (entretien + évaluation) | 0,10 – 0,40 € |
| STT (transcription) | 0,10 – 0,25 € |
| TTS (voix recruteur, ~10 min de parole) | 0,15 – 0,50 € |
| Avatar animé simple | ~0 € (rendu client) |
| **Total MVP** | **≈ 0,35 – 1,15 € / session** |
| Avatar photoréaliste temps réel (V2) | + 3 – 15 € / session — à réserver à l'offre premium |

Un garde-fou logiciel coupe la session si le budget maximal est dépassé.

---

## 8. Risques principaux et parades

| Risque | Parade |
|---|---|
| Latence vocale > 2 s casse l'immersion | Streaming partout (STT, LLM, TTS) ; l'avatar « réfléchit » (comportement d'attente naturel) pendant le traitement |
| Qualité des questions décevante | Sprint 3 dédié à l'itération sur les prompts, avec jeu d'entretiens de test et grille de notation |
| Dérive des coûts IA | Journal de coût par session + plafond automatique dès le sprint 3 |
| Dépendance à un fournisseur IA | Pattern adaptateur : changement de fournisseur sans refonte |
| Réponses inappropriées du LLM | Garde-fous dans le prompt + validation de sortie + bouton de signalement |

---

## 9. Définition de « terminé » (Definition of Done)

Une fonctionnalité est terminée quand : le code est relu et mergé sur `main` · les tests passent en CI · les entrées sont validées · les erreurs sont gérées · la documentation API est à jour · la fonctionnalité est démontrée sur l'environnement de staging.
