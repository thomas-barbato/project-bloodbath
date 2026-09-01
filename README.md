# Project Bloodbath

Prototype de FPS/ARPG développé avec **Unity 6.5 (6000.5.10f1)**.

## Structure du dépôt

- `projet bloodbath/` : projet Unity à ouvrir dans Unity Hub ;
- `docs/` : architecture technique et feuille de route ;
- `Présentation du projet.md` : vision générale et fonctionnalités prévues.

Le dépôt Git doit toujours être utilisé depuis cette racine. Le dossier Unity ne doit pas être initialisé comme un dépôt Git séparé.

## Fichiers Unity versionnés

Les dossiers `Assets`, `Packages` et `ProjectSettings` sont versionnés. Les fichiers générés localement par Unity ou l’IDE (`Library`, `Temp`, `Logs`, fichiers de solution, etc.) sont exclus par `projet bloodbath/.gitignore`.

Les ressources binaires telles que les textures, modèles et fichiers audio sont gérées avec Git LFS.
