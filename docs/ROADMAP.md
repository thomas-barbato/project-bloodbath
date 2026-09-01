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
- Modèle main principale / main secondaire avec arme à une main, arme à deux mains et premier bouclier fonctionnel.
- Premier essai du tir à deux armes : attaque principale simple et attaque simultanée des deux armes via l’action secondaire reconfigurable.
- Cible d'entraînement puis premier ennemi mobile.
- Dégâts typés, mort, projection et feedback sonore/visuel.
- Premier HUD fonctionnel pour la santé, l’arme, les compétences et les informations de combat essentielles.
- Premiers sons provisoires permettant d’évaluer les armes, impacts et menaces ennemies.

## 3. Gore modulaire

- Réactions déterminées par le type de dégâts et le coup fatal.
- Sang, decals et morceaux secondaires limités par des budgets configurables.
- Première réaction spécifique pour un humain, une machine et une créature.

## 4. Progression et builds

- Statistiques, première classe, objets et prérequis.
- Première compétence active réactive et première compétence passive transformant le gameplay.
- Première barre de compétences fonctionnelle, sans figer encore son UX finale.
- Loot, inventaire et équipement.
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

Ces systèmes seront préparés dans les données et la persistance, puis réellement produits lorsque la campagne disposera d’assez de contenu pour que leur boucle puisse être évaluée.

## 8. Expérimentation multijoueur

- Prototype host/client jusqu'à cinq joueurs.
- Validation des règles d'autorité du gameplay et des objectifs partagés.
- Personnages solo et multijoueur séparés dès leur création.
- Persistance serveur des personnages, du loot et des quêtes multijoueur, sans transfert depuis le solo.
- Validation serveur des déblocages de difficulté et de la mort des personnages Hardcore.

Le choix définitif de la pile réseau et du service de persistance reste volontairement différé jusqu'à ce que la boucle solo du vertical slice soit amusante et suffisamment stable pour être synchronisée.
