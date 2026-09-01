# Architecture initiale

## Principes

- Le gameplay commun ne connaît pas le mode de sauvegarde.
- Les entrées pilotent le personnage par une interface claire et pourront être remplacées par des entrées réseau ou une IA.
- Les valeurs de réglage sont configurables sans modifier le code central.
- Les systèmes publient des faits de gameplay ; les quêtes, l'interface, l'audio et les statistiques peuvent ensuite y réagir indépendamment.
- La présentation locale non critique, notamment une partie du gore, reste distincte de l'état autoritaire à synchroniser.

## Modules prévus

`Input` lit les périphériques locaux, sélectionne des commandes par défaut adaptées à la disposition du clavier et conserve les personnalisations clavier/souris et manette ainsi que la sensibilité choisie par le joueur. `Player` porte le moteur de déplacement et le corps. `Combat` gérera les armes à distance et de mêlée, les dégâts et les réactions. `Progression` portera statistiques, compétences actives et passives, objets, inventaire et équipement. `Narrative` décrira histoire, dialogues et lore. `Quests` décrira les définitions, objectifs, récompenses et états d'avancement. `Persistence` adaptera ces états vers une sauvegarde locale ou un service serveur. `Presentation` contiendra les effets visuels et sonores remplaçables.

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

## Décisions différées

- Solution réseau et intégration précise des lobbies Steam.
- Fournisseur de persistance et d'authentification multijoueur.
- Structure exacte des classes et statistiques.
- Scénario, factions, acte initial et protagoniste.
- Modèle final du corps, rig et pipeline d'équipement interchangeable.

Ces décisions seront prises avant le premier système qui en dépend réellement, sur la base d'un prototype déjà jouable plutôt que d'hypothèses abstraites.
