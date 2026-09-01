# Feuille de route initiale

Cette feuille de route traduit la présentation du projet en petits jalons jouables. Chaque jalon doit être testé dans l'éditeur avant d'élargir le contenu.

## 0. Fondations

- Unity 6.5 (6000.5.10f1), URP et nouveau système d'entrées.
- Code du jeu isolé sous `Assets/ProjectBloodbath`.
- Paramètres de gameplay configurables par données.
- Séparation entre logique de gameplay, présentation, entrées et persistance.
- Événements de gameplay exploitables plus tard par les quêtes sans coupler les objectifs aux ennemis, aux armes ou à l'interface.

## 1. Sensations FPS

- Arène grise de test.
- Déplacement, saut, sprint et caméra.
- Sensibilité de la souris réglable par le joueur et sauvegardée localement.
- Corps provisoire visible en première personne et représentation complète dans le monde.
- Détection de la disposition clavier, profils AZERTY/QWERTY et reconfiguration des commandes.
- Profils clavier/souris et manette séparés, persistants et adaptés au dernier périphérique utilisé.
- Validation manette/clavier-souris et premières mesures de performance.

## 2. Combat fondamental

- Première arme hitscan, munitions, cadence, recul et impacts.
- Première arme de mêlée avec portée, arc d’attaque, impact et réaction propres.
- Cible d'entraînement puis premier ennemi mobile.
- Dégâts typés, mort, projection et feedback sonore/visuel.

## 3. Gore modulaire

- Réactions déterminées par le type de dégâts et le coup fatal.
- Sang, decals et morceaux secondaires limités par des budgets configurables.
- Première réaction spécifique pour un humain, une machine et une créature.

## 4. Progression et builds

- Statistiques, première classe, objets et prérequis.
- Première compétence active réactive et première compétence passive transformant le gameplay.
- Loot, inventaire et équipement.
- Premier effet d'arme transformant le gameplay et première synergie.
- Équipement visible sur le corps du personnage.

## 5. Boucle narrative et quêtes

- Première quête principale courte et une quête secondaire.
- Objectifs data-driven alimentés par les événements de gameplay.
- Récompenses, suivi, validation et enchaînement de quêtes.
- PNJ ou terminal concis, lore facultatif et narration environnementale.
- Progression de quête indépendante des futures implémentations de persistance solo et multijoueur.

## 6. Vertical slice

- Petit niveau rétrofuturiste sombre avec un début, une montée en tension et une fin.
- Premier démon, mini-boss ou boss.
- Français source, anglais, options vidéo et interface de 1080p à 4K.
- Boucle complète : combat, loot, équipement, quête, récompense et nouveau combat.

## 7. Expérimentation multijoueur

- Prototype host/client jusqu'à cinq joueurs.
- Validation des règles d'autorité du gameplay et des objectifs partagés.
- Personnages solo et multijoueur séparés dès leur création.
- Persistance serveur des personnages, du loot et des quêtes multijoueur, sans transfert depuis le solo.

Le choix définitif de la pile réseau et du service de persistance reste volontairement différé jusqu'à ce que la boucle solo du vertical slice soit amusante et suffisamment stable pour être synchronisée.
