# Project Bloodbath

Prototype de FPS/ARPG développé avec **Unity 6.5 (6000.5.10f1)**.

## Structure du dépôt

- `Assets/`, `Packages/` et `ProjectSettings/` : projet Unity à ouvrir directement depuis la racine ;
- `docs/` : architecture technique et feuille de route ;
- `Présentation du projet.md` : vision générale et fonctionnalités prévues.

Le dépôt Git et le projet Unity partagent la même racine. Dans Unity Hub, il faut donc sélectionner directement le dossier `project-bloodbath`.

## Fichiers Unity versionnés

Les dossiers `Assets`, `Packages` et `ProjectSettings` sont versionnés. Les fichiers générés localement par Unity ou l’IDE (`Library`, `Temp`, `Logs`, `.vscode`, fichiers de solution, etc.) sont exclus par `.gitignore`.

Les ressources binaires telles que les textures, modèles et fichiers audio sont gérées avec Git LFS.
