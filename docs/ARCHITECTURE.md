# Architecture initiale

## Principes

- Le gameplay commun ne connaît pas le mode de sauvegarde.
- Les entrées pilotent le personnage par une interface claire et pourront être remplacées par des entrées réseau ou une IA.
- Les valeurs de réglage sont configurables sans modifier le code central.
- Les systèmes publient des faits de gameplay ; les quêtes, l'interface, l'audio et les statistiques peuvent ensuite y réagir indépendamment.
- La présentation locale non critique, notamment une partie du gore, reste distincte de l'état autoritaire à synchroniser.

## Modules prévus

`Input` lit les périphériques locaux, sélectionne des commandes par défaut adaptées à la disposition du clavier et conserve les personnalisations clavier/souris et manette ainsi que la sensibilité choisie par le joueur. `Player` porte le moteur de déplacement et le corps. `Combat` gérera les armes à distance et de mêlée, les dégâts, le blocage et les réactions. `Loadout` décrira les deux mains, l’occupation à une ou deux mains, les boucliers, les compatibilités d’équipement et les actions disponibles. `Progression` portera statistiques, compétences actives et passives, objets, inventaire et équipement. `Difficulty` décrira les paliers Incursion, Extermination et Bloodbath ainsi que leurs règles de montée en puissance. `WorldAccess` suivra les zones débloquées et revisitables par personnage et par difficulté. `CharacterRules` portera notamment l’état normal ou Hardcore et la conséquence permanente d’une mort. `Narrative` décrira histoire, dialogues et lore. `Quests` décrira les définitions, objectifs, récompenses et états d'avancement. `Persistence` adaptera ces états vers une sauvegarde locale ou un service serveur. `UI` transformera les états et événements utiles en HUD, barre de compétences et menus thématisés sans porter les règles de gameplay. `Audio` réagira aux événements de gameplay, gérera le mixage et pilotera les états de musique dynamique. `Presentation` regroupera les effets visuels et sonores remplaçables.

Les dépendances doivent aller des couches externes vers les règles de gameplay, jamais l'inverse. Par exemple, un ennemi publiera sa mort avec son identifiant et son contexte ; il ne cherchera pas lui-même les quêtes à mettre à jour.

## Décisions prises

- Moteur : Unity 6.5 (6000.5.10f1).
- Rendu : Universal Render Pipeline.
- Entrées : Input System avec clavier/souris et manette conservés dès le prototype.
- Les scripts de gameplay lisent des actions, jamais des touches ou boutons écrits en dur.
- Les personnalisations de commandes sont des préférences locales et non une progression de personnage.
- Déplacement initial : `CharacterController` cinématique, adapté à un FPS rapide et réglable.
- Namespace racine : `ProjectBloodbath`.
- Nom du jeu : « Project Bloodbath » reste un titre de travail.
- Interface : approche hybride FPS / ARPG, configurable et strictement intégrée à la direction artistique.
- Audio : retours de combat précoces, mixage par catégories et musique adaptative pour le vertical slice.
- Équipement : deux mains fonctionnelles, armes à une ou deux mains et boucliers protecteurs.
- Deux armes à distance : le clic droit par défaut déclenche provisoirement les deux attaques de base en même temps ; l’action reste reconfigurable.
- Difficultés : Incursion, Extermination puis Bloodbath, avec déblocage et persistance par personnage.
- Hardcore : disponible dès la création, séparé du mode normal et fondé sur une mort définitive en solo comme en multijoueur.
- Monde : les zones déjà débloquées dans la difficulté actuelle restent revisitables.
- Distribution publique : durcissement réaliste du client, notamment par IL2CPP et suppression contrôlée du code inutilisé, sans promesse de chiffrement inviolable.

## Décisions différées

- Solution réseau et intégration précise des lobbies Steam.
- Fournisseur de persistance et d'authentification multijoueur.
- Structure exacte des classes et statistiques.
- Scénario, factions, acte initial et protagoniste.
- Modèle final du corps, rig et pipeline d'équipement interchangeable.
- Direction musicale de référence et étendue exacte du doublage.
- Rythme, alternance et attaques combinées du combat de mêlée à deux armes.
- Comportement du tir simultané lorsque les deux armes ont des cadences ou états de rechargement différents.
- Valeurs exactes et variantes du blocage au bouclier.
- Nombre, disposition et apparence finale des emplacements de la barre de compétences.
- Éventuelle solution d’obfuscation et contrôles d’intégrité des builds publics.

Ces décisions seront prises avant le premier système qui en dépend réellement, sur la base d'un prototype déjà jouable plutôt que d'hypothèses abstraites.
