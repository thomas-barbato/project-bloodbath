# Project Bloodbath — Arbres de compétences et synergies

> **Version de conception 2 — révision d'équilibrage du 4 septembre 2026.**  
> Ce document décrit les 12 arbres actuellement matérialisés en `ScriptableObject` dans Unity. Les noms de classes et de compétences restent provisoires et feront l'objet d'une passe d'écriture ultérieure.

## 1. Principes communs

- Niveau maximal : **99**, avec **98 points naturels** gagnés entre les niveaux 2 et 99.
- Chaque compétence accepte **20 rangs durs**. L'équipement peut dépasser ce rang, mais ne satisfait pas les prérequis et n'augmente pas les bonus accordés aux autres compétences.
- Chaque classe possède **3 arbres de 10 compétences**, chacun composé de **5 actives et 5 passives** : **120 compétences**, dont **60 actives**.
- Le joueur dispose de **5 emplacements de barre active**. Une active investie mais non équipée continue d'accorder ses synergies de rang dur.
- Les compétences verrouillées par le niveau ou un prérequis sont assombries ; le rouge rouille signale l'échec d'une tentative d'achat, pas un code couleur permanent.
- Les effets automatiques en chaîne possèdent un plafond, un délai interne ou une profondeur secondaire nulle. Ils ne peuvent jamais s'autoalimenter indéfiniment.
- Une attaque en cône, en zone ou à rebonds possède son propre mode d'attaque ; aucune compétence ne corrige la visée des tirs ordinaires.
- Les relations ci-dessous indiquent toujours l'effet exact. Lorsqu'aucune relation de rang n'est indiquée, la compétence n'en possède pas.

Dans les tableaux, **A** signifie active, **P** passive et `R1 → R20` la progression du rang 1 au rang 20. Un tiret dans la colonne d'accès signifie que seul le niveau est requis.

# 2. Enforcer

L'Enforcer utilise trois boucles distinctes : **Garde** pour encaisser et restituer la pression, **Carnage** pour soutenir l'assaut de mêlée, et des **Ordres exclusifs** pour orienter temporairement le groupe.

## 2.1. Blindage de siège

| Niv. | Type | Compétence | Accès | Effet révisé |
|---:|:---:|---|---|---|
| 1 | A | Ancrage d'abattoir | — | S'ancre directement ; l'ancienne seconde jauge d'Inertie est supprimée au profit de l'état binaire **Ancré**. |
| 1 | P | Plaques de récupération | — | +5 % → 20 % armure, +2 % → 10 % bouclier et 5 % → 20 % de restauration à l'ancrage ; l'ancien doublon d'urgence sous 50 % de santé est supprimé. |
| 6 | A | Treuil gravifique | Ancrage d'abattoir | Attire 3 → 7 cibles et produit au plus 3 Gardes par activation. |
| 6 | P | Châssis anti-impact | Plaques de récupération | Réduit les chocs et transforme les impacts lourds en fenêtre défensive lisible. |
| 12 | A | Charge de rempart | Châssis anti-impact | Charge sur 6 m → 10 m, inflige 150 % → 360 % dans 2,5 m → 4,5 m. |
| 12 | P | Réacteur de représailles | Treuil gravifique | Porte la Garde de 6 à 10 ; chaque charge donne 0,25 % → 0,5 % de réduction et +5 % aux dégâts de Purge, plafonnés à +50 %. |
| 18 | A | Purge cinétique | Réacteur de représailles OU Charge de rempart | Consomme la Garde pour une onde dont dégâts et rayon sont plafonnés. |
| 24 | P | Dernier blindage | Plaques de récupération OU Charge de rempart | Fenêtre de 4 s → 6 s, réduction 10 % → 24 %, recharge 30 s → 18 s ; Purge rend 0,1 % → 0,4 % de santé par Garde. |
| 30 | A | Dôme de ferraille | Dernier blindage | Dôme de 6 s → 9 s et 5 m → 7 m ; réduction générale 8 % → 18 %, bonus projectiles/explosions +8 %, total plafonné à 31 %. |
| 30 | P | Masse inamovible | Purge cinétique | S'active avec au moins 1 Garde face à un boss ou 3 ennemis proches ; réduction 5 % → 15 %. |

Relations de rang dur :

- **Charge de rempart ← Châssis anti-impact :** +1,5 % de dégâts par point.
- **Purge cinétique ← Réacteur de représailles :** +1,5 % de dégâts par point, dans le plafond de Purge.
- **Dôme de ferraille ← Dernier blindage :** +1,5 % de durée par point.
- **Masse inamovible ← Purge cinétique :** +1,5 % de réduction défensive par point, dans son plafond.

## 2.2. Assaut vélocitaire

| Niv. | Type | Compétence | Accès | Effet révisé |
|---:|:---:|---|---|---|
| 1 | A | Percée d'équarrissage | — | Percée offensive qui ouvre l'engagement et alimente Carnage. |
| 1 | P | Servomoteurs agressifs | — | Soutient vitesse d'action et déplacement de mêlée. |
| 6 | A | Lacération croisée | Servomoteurs agressifs | Double coupe qui applique Trauma. |
| 6 | P | Appétit cinétique | Percée d'équarrissage | Une élimination prépare un unique **Élan d'équarrissage** pendant 2 s → 4 s ; la récupération de Percée est fixée à 30 %. |
| 12 | A | Bond de curée | Appétit cinétique | Bond de 7 m → 12 m, dégâts 130 % → 320 % et rayon 3 m → 5 m. |
| 12 | P | Moteur de carnage | Lacération croisée | Carnage reste l'unique réserve empilée, de 3 à 10 charges ; chaque charge peut rendre jusqu'à 0,75 % de santé et 4 Énergie. |
| 18 | A | Exécution hydraulique | Lacération croisée | 220 % → 460 %, +12 % par Trauma (5 max.), +3 % par Carnage (10 max.) et +35 % sous le seuil d'exécution. |
| 24 | P | Réflexe d'équarrissage | Exécution hydraulique OU Bond de curée | Rend 20 % → 50 % de la recharge de Percée et réduit de 15 % → 40 % le coût de la prochaine mêlée ; délai 8 s → 4 s. |
| 30 | A | Rotor de boucherie | Bond de curée OU Percée d'équarrissage | Rayon 4 m → 6 m, 4 → 7 impacts de 45 % → 70 % ; les frappes supplémentaires consomment Carnage. |
| 30 | P | Surcadence prédatrice | Moteur de carnage OU Réflexe d'équarrissage | À Carnage maximal, frappe secondaire 25 % → 60 % ; profondeur de chaîne secondaire : 0. |

Relations de rang dur :

- **Lacération croisée ← Servomoteurs agressifs :** +1,5 % de dégâts par point.
- **Bond de curée ← Appétit cinétique :** +1,5 % de dégâts par point.
- **Exécution hydraulique ← Lacération croisée :** +1,5 % de dégâts par point.
- **Rotor de boucherie ← Moteur de carnage :** +1,5 % de dégâts par point.
- **Surcadence prédatrice ← Exécution hydraulique :** +1 % de dégâts de frappe secondaire par point.

## 2.3. Doctrine de commandement

Les Ordres durent 8 s → 11 s, couvrent 12 m de base et sont **mutuellement exclusifs**.

| Niv. | Type | Compétence | Accès | Effet révisé |
|---:|:---:|---|---|---|
| 1 | A | Ordre : Avancez | — | Mobilité offensive et cadence d'approche. |
| 1 | P | Présence de fer | — | Renforce l'autorité et la tenue du groupe proche. |
| 6 | A | Ordre : Tenez la ligne | — | Défense collective ; profite des Plaques de récupération sans dupliquer leur calcul. |
| 6 | P | Relais vox-médullaire | Un des trois Ordres | +0,5 m → 3 m de rayon, 1,5 s → 4 s de persistance ; 1 puis 2 relais aux rangs 10 et 20, sans relais de relais. |
| 12 | A | Ordre : Écrasez-les | — | Une seule impulsion offensive par Ordre ; seuil de boss 10 %. |
| 12 | P | Chaîne de commandement | Un des trois Ordres | +0,5 % → 10 % de puissance personnelle et +1 % → 3 % par allié humain, 4 alliés maximum. |
| 18 | A | Marquage d'extermination | Un des trois Ordres | Vulnérabilité 6 % → 15 % pendant 6 s → 10 s ; un boss ne reçoit qu'une impulsion par marque. |
| 24 | P | Doctrine du survivant | Marquage d'extermination OU Chaîne de commandement | Les événements de survie prolongent un Ordre de 3 s au maximum. |
| 30 | A | Ralliement traumatique | Doctrine du survivant | Rend 10 % → 30 % de bouclier, 5 → 20 Énergie ou 5 % → 12 % de santé sans bouclier ; immunité 0,5 s → 1,25 s. |
| 30 | P | Autorité de massacre | Au moins deux Ordres | Donne 1 → 3 Gardes ou Carnages ; un seul microbonus défensif et offensif par Ordre. |

Relations de rang dur :

- **Relais vox-médullaire ← Ordres appris :** augmente rayon et persistance, sans chaîne secondaire.
- **Ordre : Tenez la ligne ← Plaques de récupération :** améliore l'armure partagée selon la valeur réellement accordée par les Plaques.
- **Marquage d'extermination ← Chaîne de commandement :** +1,5 % de vulnérabilité par point, dans le plafond de la marque.
- **Ralliement traumatique ← Doctrine du survivant :** +1,5 % de restauration par point.
- **Autorité de massacre ← Ordres appris :** améliore la ressource correspondant à l'Ordre, une fois par activation.

# 3. Marine

Le Marine repose sur **Saturation** pour les armes légères, **Fracture** pour les armes lourdes et **Amorce** pour les explosifs. Mara Voss, personnage de `MovementLab`, appartient à cette classe et ne reçoit que ces trois arbres.

## 3.1. Doctrine de saturation

| Niv. | Type | Compétence | Accès | Effet révisé |
|---:|:---:|---|---|---|
| 1 | A | Rafale terminale | — | 3 → 7 tirs comprimés, chacun à 70 % → 100 %, consommant normalement les munitions et générant Saturation sur impact. |
| 1 | P | Cadence prédatrice | — | 4 → 12 Saturations ; +0,5 % → 1,2 % cadence et +0,5 % → 1 % recharge par charge. |
| 6 | A | Rechargement de combat | — | Pas de 3 m → 6 m et recharge de 15 % → 60 % des chargeurs depuis la réserve. |
| 6 | P | Mobilité balistique | Rafale terminale OU Rechargement de combat | Élan bref non empilable ; réduit de 20 % → 60 % la perte de Saturation et de 5 % → 15 % les coûts légers. |
| 12 | A | Balayage de culasse | Cadence prédatrice OU Mobilité balistique | Cône 65° → 95°, 2 → 5 tirs par arme, 2 → 3 impacts max. par cible ; consomme 6 Saturations au plus. |
| 12 | P | Alimentation brutale | Rechargement de combat | Conserve 20 % → 80 % de Saturation et renforce les 3 → 8 premiers tirs de 5 % → 25 %. |
| 18 | A | Double détente | Rafale terminale OU Balayage de culasse | 240 % → 520 %, consomme jusqu'à 6 Saturations et fait exploser Criblé pour 80 % → 200 %. |
| 24 | P | Pluie de douilles | Double détente OU Alimentation brutale | Tous les 12e → 6e impacts à 6 Saturations, gerbe de 25 % → 70 % avec délai 0,45 s → 0,25 s par cible. |
| 30 | A | Tempête de culasse | Double détente | 5 s → 9 s de surcadence ; munitions et rechargements restent réels, prolongation par élimination plafonnée à 3 s. |
| 30 | P | Chasseur sous adrénaline | Pluie de douilles OU Mobilité balistique | À Saturation max., une élimination donne 3 s → 6 s d'Adrénaline et un plancher à 50 % de Saturation. |

Relations de rang dur :

- **Rafale terminale ← Cadence prédatrice :** +1,5 % de dégâts par point.
- **Rechargement de combat ← Mobilité balistique :** +1 % de distance par point ; **← Alimentation brutale :** +1 % de chargeur.
- **Balayage de culasse ← Cadence prédatrice :** +2 % de dégâts par point ; **← Mobilité balistique :** +1 % de portée par tranche de 5 points.
- **Double détente ← Rafale terminale :** +2 % de dégâts ; **← Balayage :** +1 % de détonation Criblé ; **← Alimentation :** +1 % de dégâts par point.
- **Pluie de douilles ← Alimentation brutale :** +1 % de largeur de gerbe par point.
- **Tempête de culasse ← Mobilité balistique / Chasseur sous adrénaline :** +0,5 % de durée par point de chaque compétence.
- **Chasseur sous adrénaline ← Double détente :** +0,5 % de durée d'Adrénaline par point.

## 3.2. Ordnance de rupture

| Niv. | Type | Compétence | Accès | Effet révisé |
|---:|:---:|---|---|---|
| 1 | A | Percuteur surchargé | — | Tir lourd préparé, munitions réellement consommées et Fracture sur impact. |
| 1 | P | Munitions à noyau dense | — | Ignore une part d'armure et augmente le stagger, sans modifier la précision. |
| 6 | A | Ligne de démolition | — | Projectile lourd pénétrant avec perte de dégâts par cible et plancher de transmission. |
| 6 | P | Affût hydraulique | Percuteur surchargé OU Ligne de démolition | Fenêtre d'ancrage au sol réduisant préparation et pénalité de déplacement. |
| 12 | A | Tir d'ancrage | Affût hydraulique | Ancre une cible et renforce le stagger contre les boss sans immobiliser ceux-ci. |
| 12 | P | Masse d'arrêt | Munitions à noyau dense OU Tir d'ancrage | Fracture plafonnée ; à son seuil, Armure rompue réduit temporairement l'armure. |
| 18 | A | Impact sismique | Tir d'ancrage OU Ligne de démolition | Onde lourde autour de l'impact ; dégâts supplémentaires sur cible inamovible. |
| 24 | P | Chambre sacrificielle | Percuteur surchargé OU Masse d'arrêt | La dernière munition déclenche une seule fois par rechargement un tir renforcé et rechambre une part du chargeur. |
| 30 | A | Surcharge de canon | Impact sismique OU Tir d'ancrage | Consomme des Fractures pour une onde plafonnée, avec bonus contre Armure rompue. |
| 30 | P | Architecture de siège | Affût hydraulique OU Chambre sacrificielle | Protocole de siège temporaire : récupération des compétences lourdes et Fracture explosive soumise à un délai par cible. |

Relations majeures : **Percuteur ← Noyau dense** (+1,5 % dégâts/point), **Tir d'ancrage ← Affût** (+1,5 % durée/point), **Impact sismique ← Masse d'arrêt** (+1,5 % dégâts/point), **Surcharge de canon ← Tir d'ancrage** (+1,5 % dégâts/point) et **Architecture de siège ← Chambre sacrificielle** (+1,5 % durée/point).

## 3.3. Dévastation contrôlée

| Niv. | Type | Compétence | Accès | Effet révisé |
|---:|:---:|---|---|---|
| 1 | A | Grenade M-13 « Écorcheuse » | — | Grenade à charges, fusée réglable et explosion physique lisible. |
| 1 | P | Ceinture de démolition | — | Augmente les charges disponibles, le rayon et réduit les dégâts explosifs personnels. |
| 6 | A | Mine charognarde | — | Mine persistante avec limite globale d'unités actives. |
| 6 | P | Composés de fosse | Grenade OU Mine | Alterner deux compétences explosives crée un unique Mélange instable non empilable. |
| 12 | A | Charge de brèche | Composés de fosse | Charge fixée ; les charges supplémentaires sur une même cible ont un rendement décroissant. |
| 12 | P | Shrapnel industriel | Ceinture OU Charge | Les éliminations explosives créent des fragments soumis à délai, profondeur secondaire nulle. |
| 18 | A | Roquette thermobarique | Charge de brèche OU Grenade | Explosion puis zone de combustion temporaire. |
| 24 | P | Réaction en chaîne | Shrapnel OU Composés | Amorce plafonnée et délai de réaction empêchant les cascades infinies. |
| 30 | A | Couronne de charges | Charge de brèche | Couronne à rayon intérieur sûr ; chaque impact ultérieur possède un bonus plafonné. |
| 30 | P | Protocole de terre brûlée | Roquette OU Réaction en chaîne | Trois compétences explosives distinctes ouvrent une fenêtre limitée de dégâts et réactions secondaires. |

Relations majeures : **Grenade ← Ceinture** (+1,5 % dégâts/point), **Mine ← Composés** (+1,5 % persistance/point), **Charge de brèche ← Shrapnel** (+1,5 % dégâts contre Déchiqueté/point), **Roquette ← Composés** (+1,5 % dégâts persistants/point), **Réaction en chaîne ← Shrapnel** (+1,5 % réaction/point) et **Terre brûlée ← Roquette** (+1,5 % durée/point).

# 4. Scientist

Le Scientist combine trois états indépendants : **Combustion**, **Cryostase** et **Conductivité**. Les réactions entre éléments sont permises, mais chaque transmission est bornée afin qu'une élimination ne puisse pas nettoyer seule toute une rencontre.

## 4.1. Thermodynamique interdite

| Niv. | Type | Compétence | Accès | Effet révisé |
|---:|:---:|---|---|---|
| 1 | A | Inciseur thermique | — | 60 % → 150 % directs, 2 → 5 Combustions, 12 % → 30 %/s pendant 4 s → 8 s. |
| 1 | P | Combustion catalytique | — | Porte Combustion de 3 à 10 charges et augmente modérément dégâts et durée. |
| 6 | A | Nappe de prométhium | — | Zone de 5 m → 7 m pendant 6 s → 8 s, 35 % → 70 %/s ; l'ancienne couverture de 9 m sur 10 s est supprimée. |
| 6 | P | Résidus pyrophages | Inciseur thermique | Une mort brûlante prépare un seul catalyseur pendant 8 s → 12 s : prochaine compétence de feu -10 % → -30 % Énergie et +10 % → +30 % durée de brûlure. |
| 12 | A | Siphon calorique | Résidus OU Combustion | Consomme au total 3 → 8 charges dans le cône ; conversion 25 % → 50 %, gain plafonné à 15 Énergie et 12 % bouclier. |
| 12 | P | Propagation pyrolytique | Nappe OU Combustion | Transmet 35 % → 70 % à 1 → 3 cibles dans 3 m → 6 m ; une brûlure transmise ne se retransmet pas. |
| 18 | A | Flashover | Inciseur OU Nappe | 100 % → 220 % dans 5 m → 7 m et conversion de 50 % → 80 % du reliquat ; profondeur secondaire 0. |
| 18 | P | Cœur de four noir | Siphon OU Propagation | Compte 3 → 6 ennemis brûlants ; chacun donne +0,5 % → +1,5 % feu et 0,1 → 0,25 Énergie/s, avec 20 % → 50 % résistance au feu. |
| 24 | A | Corona de fusion | Nappe OU Flashover | 240 % → 480 % dans 6 m → 8 m, 3 s → 4 s de persistance à 10 % → 20 %/s, 2 → 5 Combustions. |
| 30 | P | Entropie affamée | Cœur OU Flashover | Micro-explosion 40 % → 120 % dans 3 m → 4 m, consomme 2 charges, délai 0,5 s par cible, profondeur secondaire 0. |

Relations de rang dur :

- **Résidus pyrophages ← Inciseur thermique :** +1,5 % de durée de brûlure du catalyseur par point.
- **Siphon calorique ← Résidus pyrophages :** +1,5 % de conversion des dégâts restants par point.
- **Propagation pyrolytique ← Nappe de prométhium :** +0,05 m de rayon par point.
- **Flashover ← Inciseur thermique :** +1,5 % de dégâts de base par point.
- **Cœur de four noir ← Siphon calorique :** +0,005 Énergie/s par cible et par point.
- **Corona de fusion ← Flashover :** +1,5 % de dégâts initiaux par point.
- **Entropie affamée ← Flashover :** +1,5 % de conservation des Combustions par point.

## 4.2. Cryogénie de confinement

| Niv. | Type | Compétence | Accès | Effet révisé |
|---:|:---:|---|---|---|
| 1 | A | Aiguille cryogénique | — | 50 % → 130 % et 12 → 28 Cryostase. |
| 1 | P | Cryostase cumulative | — | Jauge unique sur 100 ; au seuil, gel 1,5 s → 3 s puis consommation de la jauge. |
| 6 | A | Plaque de givre | — | 5 m → 7,5 m pendant 6 s → 8 s, 30 % → 65 %/s, ralentissement 20 % → 45 %. |
| 6 | P | Pression de phase | Aiguille | Après 25 Cryostase appliquées, une charge unique donne +8 % → +25 % dégâts et +5 % → +15 % Cryostase à la prochaine glace. |
| 12 | A | Prison de saumure | Pression OU Cryostase | Prison 2,5 s → 4,5 s dans 3 m → 4,5 m ; les lourds la brisent et les boss sont immunisés. |
| 12 | P | Fragilité cristalline | Plaque OU Cryostase | +3 % → +12 % vulnérabilité physique/explosive ; rupture 60 % → 180 % dans 3 m → 5 m, profondeur secondaire 0. |
| 18 | A | Fracture cryogénique | Aiguille OU Plaque | 180 % → 380 %, +35 % → +75 % sur cible gelée, onde dans 5 m → 7 m ; consomme le gel. |
| 18 | P | Échangeur cryogénique | Prison OU Fragilité | 1 → 3 conversions par action, délai global 0,75 s ; 1 → 4 Énergie et 1 % → 3 % bouclier par déclenchement. |
| 24 | A | Zéro local | Plaque OU Fracture | 200 % → 420 % dans 5 m → 8 m, 60 → 100 Cryostase, +10 % → +20 % armure pendant 3 s → 5 s. |
| 30 | P | Mort thermique | Échangeur OU Fracture | Après rupture : vulnérabilité 8 % → 20 % pendant 4 s → 8 s et choc 80 % → 220 %, sans cascade. |

Relations de rang dur :

- **Pression de phase ← Aiguille cryogénique :** +1,5 % de Cryostase bonus par point.
- **Fragilité cristalline ← Plaque de givre :** +1,5 % de dégâts d'explosion de rupture par point.
- **Fracture cryogénique ← Fragilité cristalline :** +1,5 % de dégâts secondaires par point.
- **Échangeur cryogénique ← Prison de saumure :** +1,5 % de bouclier rendu par point.
- **Zéro local ← Fracture cryogénique :** +1,5 % de dégâts par point.
- **Mort thermique ← Fracture cryogénique :** +1,5 % de dégâts de choc thermique par point.

## 4.3. Électrocinétique de rupture

| Niv. | Type | Compétence | Accès | Effet révisé |
|---:|:---:|---|---|---|
| 1 | A | Arc galvanique | — | 90 % → 230 %, 2 → 5 cibles supplémentaires dans 5 m → 8 m, perte par rebond 20 % → 8 %. |
| 1 | P | Conductivité | — | 3 → 7 charges, +1 % → +2,5 % dégâts électriques reçus par charge, 4 s → 8 s. |
| 6 | A | Lance voltaïque | — | 180 % → 420 %, consomme jusqu'à 7 Conductivités pour +12 % → +22 % chacune ; un seul stagger. |
| 6 | P | Courant de retour | Arc galvanique | Un rebond inutilisé prépare une charge unique : prochaine électrique -10 % → -25 % coût et +8 % → +20 % dégâts. |
| 12 | A | Pas ionique | Courant OU Conductivité | Déplacement 5 m → 9 m, 100 % → 240 % à l'arrivée ; intangibilité fixée à 0,12 s. |
| 12 | P | Superconduction | Lance OU Conductivité | +10 % → +35 % portée, au plus 2 rebonds bonus, transfert de 10 % → 35 % Cryostase, sans chaîne secondaire. |
| 18 | A | Sphère de Faraday | Pas OU Lance | 6 s → 9 s dans 5 m → 7 m ; 2 → 4 cibles/s, 35 % → 65 %, délai 0,5 s par cible. |
| 18 | P | Surtension | Superconduction OU Courant | 1 → 3 arcs de 30 % → 80 % et 1 → 3 Énergie ; délai 2 s, profondeur secondaire 0. |
| 24 | A | Orage mobile | Sphère OU Pas | 7 m → 9 m pendant 5 s → 7 s ; 3 → 6 cibles/s, 70 % → 120 %, une frappe/s/cible et un seul recentrage. |
| 30 | P | Singularité voltaïque | Surtension OU Lance | Implosion 80 % → 220 % dans 3 m → 5 m ; convertit 25 % → 50 % des brûlures et transfère 2 Conductivités au plus, sans chaîne. |

Relations de rang dur :

- **Courant de retour ← Arc galvanique :** +1,5 % de dégâts de la prochaine compétence électrique par point.
- **Superconduction ← Lance voltaïque :** +1,5 % de portée de chaîne par point.
- **Surtension ← Superconduction :** +1,5 % de dégâts des arcs secondaires par point.
- **Orage mobile ← Sphère de Faraday :** +1,5 % de dégâts de frappe par point.
- **Singularité voltaïque ← Lance voltaïque :** +1,5 % de dégâts d'implosion par point.

# 5. Engineer

L'Engineer contrôle exclusivement des constructions mécaniques. Un seul compagnon principal permanent peut être actif ; les drones temporaires partagent une bande passante globale et toutes les créations récursives ont été supprimées.

## 5.1. Châssis principal

| Niv. | Type | Compétence | Accès | Effet révisé |
|---:|:---:|---|---|---|
| 1 | A | Molosse K-9 « Charognard » | — | Châssis d'assaut : 100 % → 220 % dégâts, bond 160 % → 360 % dans 2,5 m → 4,5 m. |
| 1 | P | Blindage cannibale | — | +1,5 % de vie et +1 % armure par rang ; réparation 0,5 % → 4 % sur machine détruite, délai 1 s. |
| 6 | A | Bastion H-0 « Porte-Cercueil » | — | 180 % → 340 % vie, écran 4 s → 7 s et 30 % → 50 % réduction ; les boss ne sont pas provoqués. |
| 6 | P | Interface neurale fossile | — | Priorité 1,5 s → 5 s sans précision bonus ; une attaque contextuelle au plus, délai 8 s → 4 s. |
| 12 | A | Rappel magnétique | Interface | Repositionne sur 12 m → 20 m ; ne soigne ni ne ressuscite, mais réduit de 10 % → 35 % une reconstruction en cours. |
| 12 | P | Protocoles de prédation | Un des trois châssis | +1,5 % dégâts par rang ; 0,5 % → 2 % par type de débuff, 5 types et +10 % maximum. |
| 18 | A | Suture-3 « Chirurgien » | — | Commande : 8 % → 20 % bouclier ou 4 % → 10 % santé sans bouclier, recharge 12 s → 8 s. |
| 24 | P | Noyau interchangeable | Un des trois châssis | Hérite de 5 % → 30 % du meilleur bonus élémentaire et applique l'état à 10 % → 40 % de sa puissance. |
| 30 | A | Surcharge interdite | Interface OU Prédation | 6 s → 10 s, +20 % → +45 % cadence, +15 % → +30 % vitesse ; -2 % santé/s sans descendre sous 1, un effet signature bonus max. |
| 30 | P | Reconstruction impie | Blindage OU Noyau | Retour après 25 s → 10 s avec 20 % → 65 % santé ; réductions de délai limitées à 3/s, les remplacements conservent état et recharges. |

Chaque châssis accorde aussi une identité permanente aux autres : **K-9** donne +0,75 % vitesse d'action par rang, **Bastion** +1 % vie et +0,5 % armure, **Suture-3** +0,5 % réparation reçue et +0,25 % régénération de bouclier.

Relations de rang dur : **Bastion ← Blindage** (+1,5 % réduction d'écran/point), **Rappel ← Interface** (+1,5 % puissance d'arrivée/point), **Suture ← Blindage** (+1,5 % soin de santé/point), **Surcharge ← Noyau** (+1,5 % cadence/point) et **Reconstruction ← Blindage** (+1,5 % santé de retour/point).

## 5.2. Essaim industriel

| Niv. | Type | Compétence | Accès | Effet révisé |
|---:|:---:|---|---|---|
| 1 | A | Vautours R-4 | — | 2 → 4 drones pendant 8 s → 14 s, 35 % → 75 % par salve ; réactivation = nouvelle priorité, pas de durée gratuite. |
| 1 | P | Bande passante volée | — | +1 % → +20 % durée, +0 → +4 unités et rayon d'ordre 15 m → 26 m ; les plus anciennes expirent en dépassement. |
| 6 | A | Scarabées de découpe | — | 3 → 6 drones pendant 6 s → 10 s, 30 % → 55 %/s chacun ; Corrosion au plus une fois/s. |
| 6 | P | Recyclage prédateur | Vautours OU Bande passante | Ferraille chaude : 3 % → 12 % soin ou 0,5 s → 2 s durée ; 3 stocks max., réduction de coût totale 15 %. |
| 12 | A | Foreuses S-2 « Fossoyeuses » | Recyclage OU Bande passante | 1 → 3 foreuses pendant 8 s → 14 s, 70 % → 150 % par émergence ; 2 emplacements chacune. |
| 12 | P | Batteries de charnier | Scarabées OU Bande passante | Prolongation par élimination plafonnée à 35 % de la durée initiale ; batterie consommable une fois, aucune batterie issue d'une réplication. |
| 18 | A | Essaim de maintenance | Batteries OU Foreuses | 2 → 5 drones pendant 8 s → 12 s ; le soin est celui de l'essaim entier, plafonné à 4,5 %/s. |
| 24 | P | Réplication de terrain | Recyclage OU Batteries | 8 → 4 ferrailles créent une unité à 50 % de durée ; 1 réplication/s, unité répliquée = 0 ferraille et 0 batterie. |
| 30 | A | Protocole Kamikaze | Au moins deux types offensifs | Sacrifie 4 → 8 unités de bande passante, 100 % → 240 % chacune ; foreuses x2, bouclier de maintenance plafonné à 24 %. |
| 30 | P | Nuée autonome | Réplication OU Bande passante | +0,5 % → +1,5 % par voisin, plafond 15 % ; 3 mini-drones max., aucun ne produit ferraille ou descendant. |

Relations de rang dur : **Foreuses ← Recyclage** (+1,5 % dégâts et +1,5 % durée/point), **Maintenance ← Batteries** (+1,5 % durée/point), **Réplication ← Recyclage** (+1,5 % ferraille du châssis détruit/point), **Kamikaze ← chacun des trois drones offensifs** (+1,5 % dégâts/point) et **Nuée ← Bande passante** (+1,5 % au plafond de puissance/point).

## 5.3. Guerre électronique et condamnation

| Niv. | Type | Compétence | Accès | Effet révisé |
|---:|:---:|---|---|---|
| 1 | A | Champ corrosif | — | 5 m → 7 m pendant 6 s → 9 s, 20 % → 50 %/s ; 5 Corrosions max. à -1 % → -2,5 % armure/résistance chacune. |
| 1 | P | Algorithme d'exploitation | — | +1 % → +2,5 % dégâts de machine par type de débuff, 5 types et +20 % max. ; alliés = 40 % du bonus. |
| 6 | A | Impulsion EMP | — | 100 % → 220 % dans 6 m → 10 m, Brouillage 3 s → 6 s ; machines normales neutralisées 1 s → 2 s, boss stagger seulement. |
| 6 | P | Couplage parasite | Champ corrosif | Lien 3 s → 7 s ; soin global des machines plafonné à 4 %/s et gain du propriétaire à 1 Énergie/s. |
| 12 | A | Balise d'exposition | Couplage OU Algorithme | 5 m → 8 m pendant 6 s → 10 s ; conserve 20 % → 60 % des débuffs à la sortie, 6 ferrailles max. par balise. |
| 12 | P | Propagation virale | EMP OU Algorithme | Transmet 1 → 3 types à 1 → 3 cibles dans 4 m → 7 m, 30 % → 60 % durée, 2 charges max. ; aucune retransmission. |
| 18 | A | Puits gravimétrique | EMP OU Balise | 5 m → 7 m pendant 3 s → 6 s, ralentissement 25 % → 50 % ; les boss ne sont jamais déplacés. |
| 24 | P | Protocoles de condamnation | Propagation OU Balise | 2 débuffs = Exposé : machines +5 % → +15 %, alliés +2 % → +8 % ; à 3 débuffs, anti-régénération 20 % → 40 %. |
| 30 | A | Directive Zéro | Balise OU Puits | 4 → 8 cibles normales, 6 s → 10 s ; une charge par type d'unité, 200 % → 420 % de base et 600 % final maximum. |
| 30 | P | Réseau de supplice | Condamnation OU Couplage | +0,05 s → +0,15 s par impact, +3 s max. par effet ; impulsions avec délai 1,5 s, réduction de recharge totale plafonnée à 10 %, sans chaîne. |

Relations de rang dur :

- **Couplage parasite ← Champ corrosif :** +1,5 % de durée de lien par point.
- **Balise d'exposition ← Couplage parasite :** +1,5 % de durée par point.
- **Propagation virale ← Impulsion EMP :** +1,5 % de dégâts EMP par point.
- **Puits gravimétrique ← Propagation virale :** +1,5 % de durée par point.
- **Directive Zéro ← Algorithme d'exploitation :** +1,5 % de dégâts de base par point, sous le plafond final.
- **Réseau de supplice ← Couplage parasite :** +1,5 % au plafond de prolongation par point.

# 6. Synergies coopératives structurantes

- **Enforcer + Marine :** regroupements, ancrage et Armure rompue préparent les cônes, tirs lourds et explosifs ; aucun bonus ne corrige la visée.
- **Enforcer + Scientist :** Garde et Ordres protègent les fenêtres de canalisation ; feu, gel et électricité profitent des cibles regroupées.
- **Enforcer + Engineer :** Puits, Treuil et Ordres concentrent alliés mécaniques et ennemis sans forcer les boss.
- **Marine + Scientist :** explosifs sur Combustion, impact lourd sur gel et Shrapnel dans les zones élémentaires créent des réactions à délai interne.
- **Marine + Engineer :** Directive Zéro donne une priorité commune aux drones et au tir du joueur, sans assistance de précision.
- **Scientist + Engineer :** Noyau interchangeable permet aux machines de participer aux états élémentaires ; la propagation transmise reste non récursive.

# 7. Garde-fous d'équilibrage

- Réduction de dégâts, vulnérabilité, restauration, génération de ressource, nombre de cibles et nombre d'unités possèdent un plafond explicite.
- Les boss remplacent immobilisation, provocation forcée et désactivation complète par menace, ralentissement autorisé ou stagger.
- Les effets créés par une propagation, une réplication, une micro-explosion ou un mini-drone ne peuvent pas recréer leur propre déclencheur.
- Les rangs d'équipement au-delà de 20 prolongent les valeurs numériques dans les plafonds sérialisés ; ils ne débloquent aucun comportement essentiel.
- Les robots attribuent éliminations, expérience, butin et objectifs à leur propriétaire, mais ne reçoivent jamais de précision garantie.
- Les valeurs restent des bases de prototype : leur validation finale exigera des mesures en combat solo et coopératif, notamment temps de destruction des boss, densité d'effets et économie d'Énergie.

# 8. État d'implémentation

- Les **12 arbres** et **120 compétences** sont présents sous `Assets/ProjectBloodbath/Content/Progression/Skills`.
- Chaque arbre est une donnée autonome `SkillTreeDefinition` et chaque compétence une `SkillDefinition`.
- `MovementLab` reste volontairement reliée aux **trois arbres Marine seulement**, car son personnage de test est Mara Voss, Marine.
- La barre conserve cinq emplacements et référence la compétence elle-même : une amélioration de rang est donc reflétée sans remplacer le raccourci.
- Les icônes actuelles sont des glyphes procéduraux distinctifs de remplacement. La production d'icônes dessinées et pixelisées reste une passe artistique ultérieure.
