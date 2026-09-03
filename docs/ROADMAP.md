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
- Déplacement, saut, double saut, sprint, glissade et caméra.
- Double saut configurable : le personnage dispose de deux impulsions au total, la seconde pouvant être utilisée en l’air. Le compteur se réinitialise seulement après un retour au sol afin de permettre un level design plus vertical sans autoriser des impulsions aériennes illimitées.
- Première glissade fondée sur l'élan avec posture abaissée, volume physique adapté, transition adoucie et courte mémoire d’entrée permettant de l’enchaîner naturellement après un saut.
- Jambes et posture visibles pendant la glissade en vue subjective, avec une représentation cohérente pour les autres joueurs.
- Action de glissade dédiée et reconfigurable, distincte du sprint placé sur `Shift`. Aucun accroupissement permanent n’est prévu tant qu’une mécanique ou le level design ne le justifie.
- Sensibilité de la souris et vitesse de caméra à la manette réglables et sauvegardées localement, avec inversion verticale séparée.
- Corps provisoire visible en première personne et représentation complète dans le monde.
- Mannequin provisoire replacé et reproportionné sous la caméra afin que le corps et les pieds entrent naturellement dans le champ, sans apparition commandée par un seuil ; pose dédiée conservée pendant la glissade.
- Détection automatique de la disposition clavier avec profil AZERTY ou QWERTY correspondant, choix manuel possible et reconfiguration interactive des commandes par catégories. Le menu sépare visuellement les options générales des raccourcis.
- Profils clavier/souris et manette séparés et persistants. La manette peut être entièrement désactivée afin d’ignorer ses entrées et de verrouiller les futures indications sur le clavier et la souris. La détection des conflits, les pictogrammes propres à chaque modèle de manette et l’adaptation au dernier périphérique utilisé lorsque la manette est active restent à compléter.
- Validation manette/clavier-souris et premières mesures de performance.

## 2. Combat fondamental

- Première arme hitscan, munitions, cadence, recul et impacts.
- Première arme de mêlée avec portée, arc d’attaque, impact et réaction propres.
- Modèle main droite / main gauche avec arme à une main, arme à deux mains et premier bouclier fonctionnel. Les actions d’entrée sont déjà séparées : clic gauche ou gâchette droite pour la main droite, clic droit ou gâchette gauche pour la main gauche.
- Premier essai du tir à deux armes : actionner les deux commandes permet aux deux armes de tirer ensemble lorsque leur état le permet ; la commande gauche deviendra une protection lorsqu’un bouclier y est équipé.
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
- Premières statistiques : Force, Agilité, Intelligence, Esprit et Constitution, initialisées à 10 dans le prototype, avec 5 points configurables gagnés par niveau, prérequis d’équipement fonctionnels et répartition accessible depuis le dossier de personnage.
- Fondation des statistiques secondaires : valeurs et limites configurables, modificateurs plats ou proportionnels regroupés par source et retrait exact d’un équipement, passif, buff, potion ou effet expiré. « Dégâts infligés » est la première valeur branchée via l’implant corrompu ; les formules dérivées restent à équilibrer.
- Première classe et effets dérivés des statistiques, uniquement après validation des fondations de combat et de build ; les rôles de classes ainsi que leurs arbres respectifs de compétences actives et passives restent volontairement à définir plus tard.
- Première compétence active réactive : une onde de rupture prototype en cône, consommant la ressource de compétence et possédant son propre temps de recharge.
- Première compétence passive : « Moisson sanglante » restitue de l’énergie lors d’une élimination attribuée au joueur et transforme ainsi la compétence active en boucle de combat renouvelable.
- Première barre de compétences fonctionnelle, sans figer encore son UX finale.
- Loot, inventaire et équipement.
- Premier ramassage hybride : ressources automatiques, objets importants manuels, surplus laissé au sol lorsque la capacité d’inventaire est atteinte, identification au réticule et notification du nom en haut de l’écran.
- Premiers profils de butin ennemi : munitions pour le poursuivant et objet manuel pour le tireur, avec probabilités et quantités configurables indépendamment de leur IA.
- Approvisionnement régulier en munitions par les zones, les ennemis appropriés puis les futurs magasins, sans rareté artificielle des consommables ordinaires.
- Première fondation d’équipement : l’implant corrompu peut circuler entre l’inventaire et l’emplacement `Implant`, applique un bonus de dégâts de prototype vérifiable et peut être équipé ou retiré depuis le dossier de personnage. La passe UX finale remplacera cette présentation de test sans déplacer les règles d’équipement dans l’interface.
- Premier effet d'arme transformant le gameplay et première synergie : le fusil accumule jusqu’à trois charges de rupture temporaires sur une cible et l’Onde de rupture les consume pour amplifier sa détonation. Les valeurs et l’identité de l’effet restent configurées par données.
- Première présentation d’équipement visible sur le corps : l’implant corrompu active un module provisoire fixé au torse lorsqu’il est équipé et le retire visuellement lorsqu’il quitte l’emplacement. Les associations entre identifiants d’objets, emplacements et futurs modèles restent indépendantes des règles d’inventaire.

## 5. Boucle narrative et quêtes

- Première quête principale courte : « Nettoyage préventif » se reçoit et se rend auprès d’un terminal de hub, demande d’éliminer les deux hostiles du laboratoire et accorde une récompense d’expérience unique.
- Première quête secondaire courte et facultative : « Prélèvement à risque » peut être acceptée en parallèle dès le début auprès d’une technicienne de quarantaine provisoire, demande de récupérer un échantillon contaminé précis puis de le lui rendre contre une récompense d’expérience provisoire.
- Objectifs data-driven alimentés par un flux générique d’événements de gameplay. Les ponts actuels transforment les morts attribuées au personnage local et les objets réellement ajoutés à son inventaire en événements identifiés, sans faire connaître le journal de quêtes aux ennemis, au combat ou au loot.
- Journal sérialisable, états `NotStarted`, `Active`, `ReadyToTurnIn` et `Completed`, suivi HUD, validation auprès du donneur et protection contre une récompense obtenue plusieurs fois.
- Journal de quêtes ouvrable avec `J` ou Select dans le prototype : liste simultanée des missions acceptées et terminées, catégorie, état, texte de présentation permanent, progression détaillée des objectifs et récompense. Le joueur peut choisir avec Entrée ou A laquelle est suivie sur le HUD. Le texte est lu depuis la même définition statique que le dialogue afin d’éviter les divergences. Le journal partage la gestion des écrans avec l’inventaire, la carte et les dialogues, sans interrompre la simulation.
- Chaînage data-driven par prérequis : une quête peut exiger que toutes ou au moins une des quêtes indiquées soient terminées. La validation d’un chapitre rend ainsi la suite disponible, mais ne l’accepte et ne la démarre jamais automatiquement.
- Première interaction générique de donneur de quête, déclinée à la fois sur le terminal de la quête principale et sur le PNJ provisoire de la quête secondaire. Une seule page rassemble le contexte narratif, les objectifs, leur progression et la récompense ; la même action d’interaction permet d’ouvrir puis d’accepter ou valider la quête. Le choix entre PNJ et terminal appartiendra au level design de chaque quête. Bulles, portraits, panneaux ou communications radio pourront ensuite remplacer ce rendu sans modifier la définition ni la progression de la quête.
- Première narration environnementale indépendante des quêtes : un terminal du laboratoire permet de consulter avec `E` ou X un rapport de quarantaine court et entièrement provisoire. Son contenu est porté par une définition de données réutilisable pour de futurs terminaux, notes, enregistrements ou objets examinables. La lecture neutralise seulement les commandes du joueur local et peut être refermée avec la même action.
- L’ouverture du dialogue neutralise uniquement les commandes locales et ne modifie jamais `Time.timeScale`.
- Progression de quête indépendante des futures implémentations de persistance solo et multijoueur.
- Première cartographie fonctionnelle : mini-carte dans l’angle supérieur droit et grande carte ouvrable avec `M` ou la croix directionnelle haute. Le laboratoire représente déjà le joueur par un marqueur directionnel jaune fortement contrasté, ainsi que les murs principaux, les ennemis vivants, le terminal de quête, le premier PNJ et les objets au sol avec des couleurs distinctes. Les cibles encore nécessaires à la quête suivie reçoivent un contour orange sur les deux cartes. Le nom d’un objet au sol reste masqué sur la grande carte jusqu’au survol de son marqueur afin de préserver la lisibilité. Les catégories portes, hubs et points d’intérêt restent préparées pour les prochains contenus.
- Carte ou transitions permettant de revisiter librement les zones déjà débloquées.

## 6. Vertical slice

- Petit niveau rétrofuturiste sombre avec un début, une montée en tension et une fin.
- Premier démon, mini-boss ou boss.
- Français source et architecture préparée pour l’anglais ; la traduction complète est différée jusqu’à la stabilisation des textes afin d’éviter de maintenir prématurément du contenu encore provisoire.
- Premier menu d’affichage fonctionnel ouvrable avec `Échap` ou la croix directionnelle basse : plein écran exclusif, plein écran fenêtré, fenêtré, résolutions et fréquences disponibles — dont le 3840×2160 et les formats 32:9 sur un écran compatible —, VSync, FOV de 60° à 120°, taille, couleur et forme du réticule. Les formes initiales sont la croix droite `+`, le point, la croix en X, le cercle et le chevron `^`. Les modifications sont explicitement appliquées ou annulées, sauvegardées localement après validation et ne mettent jamais le monde en pause. La mise à l’échelle du menu est prévue de 1080p à 4K ainsi qu’en 3840×1080 et 5120×1440 sans étirement horizontal.
- `Échap` ouvre désormais un menu système parent avec Reprendre, Sauvegarder et quitter, Vidéo, Son et Contrôle. Vidéo et Contrôle ouvrent leurs sous-menus fonctionnels ; Son reste préparé pour la future passe audio. Les sous-menus partagent la même coordination d’interface et le monde n’est pas mis en pause, y compris en solo, afin de conserver les règles du multijoueur.
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
