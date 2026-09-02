# Architecture initiale

## Principes

- Le gameplay commun ne connaît pas le mode de sauvegarde.
- Les entrées pilotent le personnage par une interface claire et pourront être remplacées par des entrées réseau ou une IA.
- Les valeurs de réglage sont configurables sans modifier le code central.
- Les systèmes publient des faits de gameplay ; les quêtes, l'interface, l'audio et les statistiques peuvent ensuite y réagir indépendamment.
- La présentation locale non critique, notamment une partie du gore, reste distincte de l'état autoritaire à synchroniser.

## Modules prévus

`Input` lit les périphériques locaux, sélectionne des commandes par défaut adaptées à la disposition du clavier et conserve les personnalisations clavier/souris et manette ainsi que la sensibilité choisie par le joueur. `Player` porte le moteur de déplacement et le corps. `Combat` gérera les armes à distance et de mêlée, les dégâts, le blocage et les réactions. `Loadout` décrira les deux mains, l’occupation à une ou deux mains, les boucliers, les compatibilités d’équipement et les actions disponibles. `Progression` portera statistiques, compétences actives et passives, objets, inventaire et équipement. `Difficulty` décrira les paliers Incursion, Extermination et Bloodbath ainsi que leurs règles de montée en puissance. `WorldAccess` suivra les zones débloquées et revisitables par personnage et par difficulté. `WorldSession` portera la population temporaire des zones et distinguera la remise en place d’une nouvelle session des réapparitions spéciales configurées. `CharacterRules` portera notamment l’état normal ou Hardcore, l’état d’âme et la récupération d’un corps. `Narrative` décrira histoire, dialogues et lore. `Quests` décrira les définitions, objectifs, récompenses et états d'avancement. `Persistence` adaptera les états durables et les récupérations de corps en cours vers une sauvegarde locale ou un service serveur, sans rendre permanents les monstres ordinaires vaincus pendant une session. `UI` transformera les états et événements utiles en HUD, barre de compétences et menus thématisés sans porter les règles de gameplay. `Audio` réagira aux événements de gameplay, gérera le mixage et pilotera les états de musique dynamique. `Presentation` regroupera les effets visuels et sonores remplaçables.

Les dépendances doivent aller des couches externes vers les règles de gameplay, jamais l'inverse. Par exemple, un ennemi publiera sa mort avec son identifiant et son contexte ; il ne cherchera pas lui-même les quêtes à mettre à jour.

## Décisions prises

- Moteur : Unity 6.5 (6000.5.10f1).
- Rendu : Universal Render Pipeline.
- Entrées : Input System avec clavier/souris et manette conservés dès le prototype.
- Les scripts de gameplay lisent des actions, jamais des touches ou boutons écrits en dur.
- Les personnalisations de commandes sont des préférences locales et non une progression de personnage.
- Déplacement initial : `CharacterController` cinématique, adapté à un FPS rapide et réglable.
- Glissade : action dédiée et reconfigurable, distincte du sprint, avec seuil d’élan, volume physique abaissé, transition de posture adoucie et courte mémoire d’entrée avant l’atterrissage. La présentation corporelle reste séparée de la logique de déplacement. Aucun accroupissement permanent n’est conservé sans besoin futur démontré par le gameplay ou le level design.
- Corps en vue subjective : le mannequin reste rendu en permanence et ne dépend d’aucun seuil angulaire. Torse, bassin et jambes partagent le même axe vertical ; seuls les pieds s’étendent vers l’avant. Cet ensemble est placé sous la caméra, avec les yeux très légèrement en avant de l’axe corporel, afin que la silhouette entre progressivement dans le champ lorsque le joueur baisse la tête. La glissade modifie sa pose sans substituer un autre corps. Les colliders appartenant au joueur restent ignorés par ses propres tirs et interactions, indépendamment de cette présentation.
- Namespace racine : `ProjectBloodbath`.
- Nom du jeu : « Project Bloodbath » reste un titre de travail.
- Interface : approche hybride FPS / ARPG, configurable et strictement intégrée à la direction artistique.
- Audio : retours de combat précoces, mixage par catégories et musique adaptative pour le vertical slice.
- Combat ennemi : les valeurs et le mode de livraison d’une attaque — impact direct ou projectile — sont portés par un profil data-driven associé à une arme ou une capacité, et non écrits directement dans l’IA.
- Comportement ennemi : perception, champ de vision, masque d’obstacles, territoire, style de déplacement et distances préférées sont portés par un profil data-driven distinct du profil d’attaque. Le contrôleur ne conserve que l’état momentané : attente, poursuite, repositionnement, combat ou retour.
- Boss : les futurs comportements pourront sélectionner et enchaîner plusieurs profils d’attaque au sein de patterns et de phases.
- Vie du joueur : la campagne déléguera la résolution au mode de jeu : mort définitive en Hardcore ou réapparition non-Hardcore sous forme d’âme au dernier hub, avec corps et totalité des possessions récupérables sur le lieu de la mort. L’âme est invulnérable et non ciblable par l’IA, mais reste une représentation visible et synchronisable pour les autres joueurs.
- Réincarnation : le contact avec le corps restaure la vie et la ressource de compétence au maximum, rend toutes les possessions et applique une source unique de malus de dégâts à 50 % pendant une durée configurable. Réappliquer cette source renouvelle sa durée sans multiplier sa valeur.
- Sessions : la mort du joueur ne réinitialise aucune rencontre. Une nouvelle session repeuple les monstres ordinaires, tandis que les réapparitions sans changement de session restent des exceptions data-driven réservées aux points, événements ou boss explicitement configurés.
- Laboratoire : `MovementLab` utilise explicitement un profil de réapparition chronométrée de 2,5 secondes pour ses deux ennemis. Ce profil est un outil de test et ne constitue pas la valeur par défaut des rencontres de campagne.
- Sauvegarde : progression automatique de type ARPG, sans retour manuel à un état antérieur. L’état durable du personnage, du monde débloqué et d’un éventuel corps non récupéré reste séparé de la population temporaire de la session.
- Équipement : deux mains fonctionnelles, armes à une ou deux mains et boucliers protecteurs.
- Deux armes à distance : le clic droit par défaut déclenche provisoirement les deux attaques de base en même temps ; l’action reste reconfigurable.
- Munitions : chaque arme à distance possède son propre chargeur, sa réserve et son état de rechargement. Ces états restent portés par l’instance d’arme afin de préparer le combat à deux armes, l’équipement et la synchronisation future.
- Ressource de compétence : un réservoir générique distinct de l’interface porte la valeur courante, le maximum, la dépense et la restauration. Son nom affiché reste provisoirement « Énergie » jusqu’au choix du vocabulaire définitif.
- Compétences actives : leurs coûts, temps de recharge, portée et effets chiffrés sont configurés par données. Le premier prototype est une onde en cône déclenchée par une action reconfigurable, distincte de l’arme tenue.
- Onde en cône : la recherche initiale couvre toute la portée autour de son origine, puis valide précisément l’angle et la ligne de vue pour chaque collider. Une cible n’est mémorisée comme traitée qu’après une ligne de vue valide, afin qu’une partie de son corps masquée n’empêche pas une autre partie visible de recevoir l’effet.
- Événements de mort : `Health` publie la cible vaincue et le coup final, dont sa source. Les passifs, puis le loot, l’expérience et les quêtes peuvent réagir à ce fait sans être appelés par l’ennemi.
- Niveau et expérience : `CharacterProgression` conserve le niveau et l’expérience courante, tandis qu’une définition séparée fournit le niveau maximal, la courbe nécessaire et les points d’attribut accordés par niveau. Chaque famille d’ennemi référence son propre profil de récompense ; le composant d’attribution remonte depuis la source du coup final jusqu’au personnage responsable et ignore les éliminations étrangères. L’excédent est conservé et peut traverser plusieurs niveaux. La future animation englobant le joueur et son message de montée de niveau écouteront `LevelChanged` sans participer au calcul.
- Statistiques principales : Force, Agilité, Intelligence, Esprit et Constitution sont des définitions de données indépendantes. `CharacterStatistics` conserve leurs valeurs et les points non dépensés, reçoit les points de chaque montée de niveau puis valide leur dépense. Les effets dérivés de ces statistiques seront ajoutés séparément.
- Statistiques secondaires : `CharacterSecondaryStatistics` agrège des modificateurs plats, additifs en pourcentage et multiplicatifs en pourcentage. Chaque ensemble porte un identifiant de source stable — équipement, passif, buff, potion, statistique principale ou effet de statut — et peut être remplacé ou retiré indépendamment. Les définitions portent leur valeur de base et leurs limites ; les formules reliant les cinq statistiques principales aux valeurs secondaires restent des données d’équilibrage à définir.
- Événements de dégâts : `Health` publie également chaque perte de vie réellement appliquée. La présentation des nombres de dégâts écoute cet événement, ne retient que les attaques du joueur contre un ennemi et place systématiquement le nombre au-dessus de la tête de la cible, indépendamment du point d’impact.
- Compétences passives : elles s’abonnent aux événements utiles et appliquent leurs effets configurés sans commande d’activation. « Moisson sanglante » vérifie que la source du coup final appartient bien au joueur avant de restaurer sa ressource.
- Inventaire de ressources : les réserves de munitions appartiennent au personnage et les armes consomment le type de munition référencé par leurs données. Chaque ressource définit sa capacité ; un ramassage accepte seulement la place disponible et conserve physiquement son surplus dans le monde.
- Économie des munitions : les munitions ordinaires restent courantes dans les zones et les butins adaptés, et pourront être proposées par les futurs magasins. Leur limite de transport structure la gestion de ressource sans en faire un loot artificiellement rare.
- Ramassage : les ressources courantes peuvent tenter un ramassage automatique au contact, tandis que les objets importants attendent une action `Interagir` reconfigurable. Tout ramassage réussi publie une notification locale contenant le nom et la quantité réellement obtenue.
- Équipement de personnage : un objet ramassable peut référencer une définition d’équipement indépendante qui porte son emplacement, ses prérequis de statistiques et ses modificateurs secondaires. `CharacterEquipment` vérifie les prérequis avant de déplacer l’objet entre l’inventaire et l’emplacement, remplace la source de modificateurs associée à cet emplacement, puis rend à l’inventaire l’objet remplacé ou retiré. Les modificateurs persistants d’équipement restent distincts des effets temporaires, comme le malus de réincarnation, afin que leurs sources puissent se combiner explicitement.
- Butin ennemi : chaque type d’ennemi référence un profil contenant des prefabs de ramassage, une probabilité et une plage de quantité. Un composant distinct réagit à la mort et instancie les résultats ; ni l’IA ni `Health` ne connaissent la table de butin. Les valeurs actuelles de `MovementLab` sont garanties pour rendre le prototype facilement vérifiable et seront équilibrées plus tard.
- Identification au sol : la caméra recherche les ramassages visibles dans une zone d’assistance autour du réticule, les classe par proximité à la visée puis par distance et transmet le meilleur candidat à la présentation. La portée d’identification est plus généreuse que celle du ramassage manuel et une aide de proximité couvre les objets près des pieds lorsque la caméra regarde vers le bas. Les colliders appartenant au joueur local sont ignorés par la ligne de vue, contrairement au décor et aux obstacles externes. Cette sélection tolérante ne ramasse rien par elle-même et reste distincte de l’inventaire.
- HUD de prototype : une présentation remplaçable lit les états publics de santé, de ressource de compétence, d’équipement, de munitions et de réincarnation sans porter ni modifier leurs règles. L’interface finale ne dépendra donc pas des composants IMGUI employés pour les essais.
- Nombres de dégâts : leur affichage flottant est une préférence locale activée par défaut et désactivable. Sa durée, sa montée, son apparence et sa limite simultanée sont configurables sans modifier le calcul des dégâts.
- Coups critiques : leur règle de calcul et leur probabilité seront ajoutées avec les statistiques. Les nombres de dégâts prévoiront alors un style et un effet visuel distincts sans intégrer cette règle dans l’interface.
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
- Format exact des patterns de boss, règles de transition entre phases et système de sélection tactique des attaques.

Ces décisions seront prises avant le premier système qui en dépend réellement, sur la base d'un prototype déjà jouable plutôt que d'hypothèses abstraites.
