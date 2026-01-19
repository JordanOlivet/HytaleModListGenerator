# Script de Listage des Mods Hytale - État du Projet

**Date :** 2026-01-19 15:30
**Version actuelle :** 0.95

---

## Objectif du Script

Générer automatiquement une liste Markdown de tous les mods installés avec leurs URLs CurseForge, en utilisant :
1. Les informations du `manifest.json` de chaque mod
2. L'API CurseForge pour trouver les URLs manquantes
3. Un système de meta-batching intelligent (par tranches de 1000 mods)
4. Une recherche par auteur pour un matching plus précis

---

## Ce qui Fonctionne (testé sur Linux)

### Version complète
- Extraction du `manifest.json` de tous les fichiers `.jar` et `.zip`
- Parsing des métadonnées : nom, version, auteur, description, website
- Détection automatique des URLs CurseForge dans le manifest
- Génération d'un fichier `mods_list.md` avec tri alphabétique
- Chargement de la clé API depuis `.api_key` (évite les problèmes d'échappement)
- Système de meta-batching (recherche par tranches de 1000 mods)
- Cache intelligent (validité 7 jours)
- Filtre pour ne matcher que les mods (pas les worlds/maps)
- Recherche par auteur comme stratégie prioritaire
- Options `--help`, `--install-deps`, `--dry-run`, `--force-refresh`

**Résultat sur 49 mods :**
- 13 mods ont une URL CurseForge directe dans le manifest
- 35/36 mods trouvés via l'API (97%)
- 1 mod non trouvé : `MobCatcher` (nom différent sur CurseForge: "Mob Capture")

---

## Bugs Corrigés (session du 2026-01-19)

1. **Clé API avec caractères spéciaux ($)** : Stockage dans `.api_key` + curl config file
2. **Index API mal calculé** : Utilisait page number au lieu d'offset
3. **Séparateur `|` dans les données** : Changé pour `@@@`
4. **IFS multi-caractères** : Bash ne supporte que 1 char, utilisation de `awk -F'@@@'`
5. **Faux positifs** :
   - "Home" matchait "cliff-side-home" (un world) → filtrage par `/mods/` dans l'URL
   - "AreaDepositorMod" matchait "sit" → longueur minimale de 5 chars pour substring match
6. **jq 1.6** : Parenthèses nécessaires autour des expressions `//`

---

## Ce qui Reste à Finaliser

### Matching Levenshtein (fuzzy matching)

**Problème** : Certains mods ont un nom très différent sur CurseForge
- Exemple : `MobCatcher` (manifest) vs `Mob Capture` (CurseForge)
- Normalisation : `mobcatcher` vs `mobcapture` → 60% de similarité

**Implémentation en cours** :
- Fonction `levenshtein_distance()` et `levenshtein_similarity()` implémentées en awk
- Seuil de similarité : 60%
- Appliqué uniquement dans la recherche par auteur (2ème passe)

**Problème de performance** :
- L'algorithme Levenshtein en awk est lent (appel awk à chaque comparaison)
- Le script devient très long avec beaucoup de mods à comparer

**Pistes d'amélioration** :
1. Implémenter Levenshtein en Python/Perl pour la performance
2. Utiliser une approche hybride : script bash + helper Python pour le fuzzy
3. Pré-calculer les distances une seule fois par auteur
4. Utiliser un seuil plus strict pour réduire les comparaisons

---

## Fichiers du Projet

```
HytaleModListGenerator/
├── list_mods.sh          # Script principal (~900 lignes)
├── .api_key              # Clé API CurseForge (texte brut)
├── .env                  # Alternative pour la clé API
├── mods/                 # Dossier contenant les mods
├── mods_list.md          # Fichier Markdown généré
├── .mods_cache.json      # Cache API (créé automatiquement)
└── README.md             # Ce fichier
```

---

## Utilisation

```bash
# Aide
./list_mods.sh --help

# Test sans créer de fichier
./list_mods.sh --dry-run

# Exécution normale
./list_mods.sh

# Forcer le rafraîchissement du cache
./list_mods.sh --force-refresh
```

---

## Architecture des Stratégies de Matching

### Ordre de priorité

1. **URL dans le manifest** : Si le mod a déjà une URL CurseForge
2. **Cache** : Si le mod a été trouvé précédemment
3. **Recherche par auteur** (Stratégie 1) :
   - Pour chaque auteur unique, recherche ses mods sur CurseForge
   - Pass 1 : Matching strict (exact, slug, substring)
   - Pass 2 : Matching fuzzy Levenshtein (>=60%) - LENT
4. **Recherche globale** (Stratégie 2) :
   - Parcours de tous les mods CurseForge par batches de 50
   - Matching strict uniquement (pas de fuzzy pour éviter la lenteur)

### Critères de matching strict

1. Nom normalisé identique (`adminui` == `adminui`)
2. Slug identique (`admin-ui` == `admin-ui`)
3. Sous-chaîne (min 5 chars) : `bettermap` contient `map` → non (trop court)

---

## Configuration API

```bash
HYTALE_GAME_ID=70216
BATCH_SIZE=50                # Taille d'une requête API (max 50)
META_BATCH_SIZE=1000         # Meta-batch (1000 mods)
MAX_API_LIMIT=10000          # Limite absolue
CACHE_VALIDITY_DAYS=7        # Validité du cache
```

---

## Notes pour la Prochaine Session

### Priorité 1 : Optimiser le fuzzy matching
Le Levenshtein en awk est trop lent. Options :
- Créer un helper Python : `levenshtein.py "str1" "str2"` → renvoie similarité
- Ou utiliser `python3 -c "from difflib import SequenceMatcher; ..."`
- Ou accepter que MobCatcher soit un cas manuel

### Priorité 2 : Fichier de mapping manuel
Pour les cas vraiment atypiques, créer `.manual_mappings.json` :
```json
{
  "MobCatcher": "https://www.curseforge.com/hytale/mods/mob-capture"
}
```

### Code du Levenshtein actuel (à optimiser)
```bash
# Fonction dans list_mods.sh lignes 288-337
levenshtein_distance() { ... }  # Calcule la distance d'édition
levenshtein_similarity() { ... } # Convertit en pourcentage (0-100)
```

---

## Résultat Actuel

```
49 mods traités
- 13 avec URL dans manifest
- 35 trouvés via API (11 par auteur + 24 par batch global)
- 1 non trouvé : MobCatcher (similarité 60% avec "Mob Capture")
```

---

**Dernière mise à jour :** 2026-01-19 15:30
**Testé sur :** Linux (Debian 11)
