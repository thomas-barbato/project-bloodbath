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
- Déplacement, saut, sprint, glissade et caméra.
- Première glissade fondée sur l'élan avec posture abaissée, volume physique adapté, transition adoucie et courte mémoire d’entrée permettant de l’enchaîner naturellement après un saut.
- Jambes et posture visibles pendant la glissade en vue subjective, avec une représentation cohérente pour les autres joueurs.
- Action de glissade dédiée et reconfigurable, distincte du sprint placé sur `Shift`. Aucun accroupissement permanent n’est prévu tant qu’une mécanique ou le level design ne le justifie.
- Sensibilité de la souris réglable par le joueur et sauvegardée localement.
- Corps provisoire visible en première personne et représentation complète dans le monde.
- Mannequin provisoire replacé et reproportionné sous la caméra afin que le corps et les pieds entrent naturellement dans le champ, sans apparition commandée par un seuil ; pose dédiée conservée pendant la glissade.
- Détection de la disposition clavier, profils AZERTY/QWERTY et reconfiguration des commandes.
- Profils clavier/souris et manette séparés, persistants et adaptés au dernier périphérique utilisé.
- Validation manette/clavier-souris et premières mesures de performance.

## 2. Combat fondamental

- Première arme hitscan, munitions, cadence, recul et impacts.
- Première arme de mêlée avec portée, arc d’attaque, impact et réaction propres.
- Modèle main principale / main secondaire avec arme à une main, arme à deux mains et premier bouclier fonctionnel.
- Premier essai du tir à deux armes : attaque principale simple et attaque simultanée des deux armes via l’action secondaire reconfigurable.
- Cible d'entraînement puis premier ennemi mobile.
- Premier profil de comportement ennemi configurable : détection, poursuite, territoire et retour au point d’origine.
- Deuxième archétype prototype maintenant sa distance et utilisant un projectile esquivable.
- Première attaque ennemie lisible avec préparation, impact évitable et récupération, pilotée par un profil associé à l’arme ou à la capacité.
- Dégâts typés, mort, projection et feedback sonore/visuel.
- Nombres de dégâts optionnels au-dessus de la tête de l’ennemi touché, montant puis disparaissant sans dépendre du point d’impact.
- Variante visuelle plus marquée des nombres lors des futurs coups critiques, après l’ajout de leur règle de calcul.
- Prototype de mort non-Hardcore : retour sous forme d’âme invulnérable et non ciblable au dernier hub, corps laissé sur place et récupération de toutes les possessions au contact.
- Réincarnation restaurant entièrement vie et ressource de compétence, puis appliquant pendant quelques secondes un malus de dégâts de 50 % non cumulable.
- La mort du joueur conserve les ennemis déjà vaincus pendant la session ; les réapparitions spéciales d’ennemis restent configurables par rencontre.
- Dans la scène `MovementLab` uniquement, les ennemis prototypes réapparaissent après 2,5 secondes afin de permettre des essais de combat répétés sans relancer la scène.
- Premier HUD fonctionnel pour la santé, la ressource de compétence, l’arme, les munitions, le rechargement et les états liés à la mort. La barre de compétences sera ajoutée avec le premier prototype de compétence active.
- Premiers sons provisoires permettant d’évaluer les armes, impacts et menaces ennemies.

## 3. Gore modulaire

- Réactions déterminées par le type de dégâts et le coup fatal.
- Sang, decals et morceaux secondaires limités par des budgets configurables.
- Première réaction spécifique pour un humain, une machine et une créature.

## 4. Progression et builds

- Première progression de personnage : niveau et expérience visibles dans le HUD de prototype, courbe et plafond configurables, excédent conservé entre plusieurs montées de niveau et récompenses distinctes par profil d’ennemi.
- Animation future de montée de niveau englobant le personnage et message dédié ; sa production visuelle est différée, mais son déclenchement restera branché sur l’événement de progression.
- Premières statistiques : Force, Agilité, Intelligence, Esprit et Constitution, initialisées à 10 dans le prototype, avec 5 points configurables gagnés par niveau et prérequis d’équipement fonctionnels. L’interface de répartition reste à produire.
- Fondation des statistiques secondaires : valeurs et limites configurables, modificateurs plats ou proportionnels regroupés par source et retrait exact d’un équipement, passif, buff, potion ou effet expiré. « Dégâts infligés » est la première valeur branchée via l’implant corrompu ; les formules dérivées restent à équilibrer.
- Première classe et effets dérivés des statistiques.
- Première compétence active réactive : une onde de rupture prototype en cône, consommant la ressource de compétence et possédant son propre temps de recharge.
- Première compétence passive : « Moisson sanglante » restitue de l’énergie lors d’une élimination attribuée au joueur et transforme ainsi la compétence active en boucle de combat renouvelable.
- Première barre de compétences fonctionnelle, sans figer encore son UX finale.
- Loot, inventaire et équipement.
- Premier ramassage hybride : ressources automatiques, objets importants manuels, surplus laissé au sol lorsque la capacité d’inventaire est atteinte, identification au réticule et notification du nom en haut de l’écran.
- Premiers profils de butin ennemi : munitions pour le poursuivant et objet manuel pour le tireur, avec probabilités et quantités configurables indépendamment de leur IA.
- Approvisionnement régulier en munitions par les zones, les ennemis appropriés puis les futurs magasins, sans rareté artificielle des consommables ordinaires.
- Première fondation d’équipement : l’implant corrompu peut circuler entre l’inventaire et l’emplacement `Implant` et applique un bonus de dégâts de prototype vérifiable. L’écran d’inventaire et l’action joueur permettant de l’équiper seront construits lors de la passe UX dédiée.
- Premier effet d'arme transformant le gameplay et première synergie.
- Équipement visible sur le corps du personnage.

## 5. Boucle narrative et quêtes

- Première quête principale courte et une quête secondaire.
- Objectifs data-driven alimentés par les événements de gameplay.
- Récompenses, suivi, validation et enchaînement de quêtes.
- PNJ ou terminal concis, lore facultatif et narration environnementale.
- Progression de quête indépendante des futures implémentations de persistance solo et multijoueur.
- Carte ou transitions permettant de revisiter librement les zones déjà débloquées.

## 6. Vertical slice

- Petit niveau rétrofuturiste sombre avec un début, une montée en tension et une fin.
- Premier démon, mini-boss ou boss.
- Français source, anglais, options vidéo et interface de 1080p à 4K.
- Première passe d’interface particulièrement soignée et visuellement intégrée à l’univers.
- Ambiance sonore cohérente et première musique dynamique entre exploration, tension et combat.
- Boucle complète : combat, loot, équipement, quête, récompense et nouveau combat.

## 7. Rejouabilité et règles de personnage

- Difficultés successives Incursion, Extermination et Bloodbath, débloquées par personnage.
- Conservation du niveau, du build et de l’équipement lors du passage à la difficulté suivante.
- Redémarrage de la campagne et montée en puissance data-driven des zones, ennemis et récompenses.
- Création d’un personnage Hardcore dès le début et validation complète de la mort définitive.
- Rejouabilité illimitée en Bloodbath après la fin de la dernière difficulté.
- Sauvegarde automatique du personnage, des déblocages et des corps non récupérés, sans sauvegarde manuelle permettant un retour en arrière.
- Nouvelle session repeuplant les zones en monstres ordinaires sans effacer la progression permanente.

Ces systèmes seront préparés dans les données et la persistance, puis réellement produits lorsque la campagne disposera d’assez de contenu pour que leur boucle puisse être évaluée.

## 8. Expérimentation multijoueur

- Prototype host/client jusqu'à cinq joueurs.
- Validation des règles d'autorité du gameplay et des objectifs partagés.
- Personnages solo et multijoueur séparés dès leur création.
- Persistance serveur des personnages, du loot et des quêtes multijoueur, sans transfert depuis le solo.
- Validation serveur des déblocages de difficulté et de la mort des personnages Hardcore.

Le choix définitif de la pile réseau et du service de persistance reste volontairement différé jusqu'à ce que la boucle solo du vertical slice soit amusante et suffisamment stable pour être synchronisée.
