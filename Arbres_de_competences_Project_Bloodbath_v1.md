# Project Bloodbath — Arbres de compétences et synergies

> Version de conception 1 — valeurs chiffrées provisoires destinées au prototypage et à l'équilibrage.
>
> **Archive :** ce document conserve l'historique de la première proposition et n'est plus une source active pour les assets Unity. La topologie et les valeurs courantes, notamment celles de **Doctrine de saturation**, sont maintenues dans `Arbres_de_competences_Project_Bloodbath_v2.md`.

## 1. Règles communes

### Progression

- Niveau maximal du personnage : **99**.
- Le personnage commence au niveau 1 sans point dépensable et reçoit **1 point de compétence à chaque montée de niveau**, soit **98 points naturels** du niveau 2 au niveau 99.
- Variante possible : accorder 1 point à la création porterait le total naturel à 99. La présente version est équilibrée autour de 98 points.
- Chaque compétence accepte **20 points investis naturellement**.
- Les objets peuvent faire dépasser le rang 20. Le système doit donc distinguer :
  - le **rang investi** : de 0 à 20 ;
  - le **rang effectif** : rang investi + bonus d'équipement.
- Les prérequis et les bonus de synergie utilisent uniquement les **points réellement investis**. Les bonus d'équipement améliorent l'effet direct de la compétence, mais ne renforcent pas ses synergies et ne permettent pas de contourner les prérequis.
- Un bonus générique `+X à une compétence` n'agit que si le personnage y a investi au moins 1 point. Un objet unique pourra exceptionnellement porter l'effet distinct `Accorde la compétence`, s'il doit permettre son utilisation sans point investi.

### Structure des arbres

Chaque classe possède trois arbres de huit compétences :

- **4 compétences actives** ;
- **4 compétences passives** ;
- déblocages aux niveaux **1, 6, 12, 18, 24 et 30** ;
- deux chaînes parallèles dans chaque arbre : une chaîne active et une chaîne passive.

```text
Niveau 1 : Actif I  ─► Niveau 6 : Actif II ─► Niveau 18 : Actif III ─► Niveau 30 : Actif IV
Niveau 1 : Passif I ─► Niveau 12 : Passif II ─► Niveau 24 : Passif III ─► Niveau 30 : Passif IV
```

Un point dans la compétence précédente suffit à ouvrir la suivante lorsque le niveau demandé est atteint. Il n'existe pas, dans cette première version, de verrou exigeant un nombre total de points dépensés dans l'arbre.

Cette structure donne **24 compétences par classe** et **96 compétences au total**. Un personnage de niveau 99 peut, par exemple, maximiser quatre compétences pour 80 points et répartir les 18 points restants entre prérequis, utilitaires et synergies.

### Barre active et rythme FPS

- Le joueur possède **5 emplacements actifs**.
- Un arbre pur fournit quatre actives ; le cinquième emplacement favorise donc naturellement une compétence utilitaire ou hybride issue d'un autre arbre.
- Les compétences doivent pouvoir être lancées en mouvement, sauf exception très courte et explicitement lisible.
- Les préparations longues, verrouillages de caméra et animations retirant le contrôle sont évités.
- Aucune compétence ci-dessous n'augmente la précision, l'assistance de visée ou la stabilité du réticule.

### Familles de valeurs

- Les compétences martiales utilisent généralement un pourcentage des **dégâts de l'arme compatible équipée**.
- Les compétences du Scientist et une partie de celles de l'Engineer utilisent la **puissance technologique**, elle-même dérivée des statistiques et de l'équipement.
- Les nombres indiqués sous la forme `R1 → R20` sont des bases de test, pas des valeurs finales.
- Au-delà du rang 20, les courbes continuent grâce aux bonus d'objets. Un rendement réduit après 20 pourra être ajouté si les essais montrent que les bonus d'équipement deviennent trop dominants.

### Règles de synergie

Les arbres utilisent quatre formes de synergie :

1. **Synergie de points investis** : une compétence reçoit un bonus explicite par point naturel placé dans une autre compétence.
2. **Synergie de statut** : une compétence applique une marque, une charge ou un état qu'une autre compétence exploite.
3. **Synergie de consommation** : une compétence transforme ou consomme les charges créées par une autre.
4. **Synergie croisée** : deux arbres différents produisent une réaction ou une boucle de gameplay commune.

Les bonus de points investis sont additionnés à l'intérieur d'une même compétence avant les multiplicateurs globaux d'équipement, afin d'éviter les combinaisons exponentielles incontrôlables.

### Cibles alliées et robots

- Les améliorations de groupe affectent les joueurs alliés à pleine puissance.
- Le compagnon principal de l'Engineer reçoit par défaut **75 %** de leur puissance.
- Les drones temporaires reçoivent par défaut **40 %** de leur puissance, afin d'éviter qu'un groupe de cinq joueurs ne multiplie excessivement les effets.
- Tous les compagnons invoqués par l'Engineer sont des **robots, automates ou drones**. Aucun cadavre, esprit, animal ou démon n'est invoqué comme familier.

---

# 2. Enforcer

L'Enforcer transforme son armure, ses servomoteurs et sa brutalité physique en trois mécaniques :

- **Garde** pour encaisser puis restituer la pression reçue ;
- **Carnage** pour entretenir les enchaînements de mêlée ;
- **Ordres** pour modifier temporairement le comportement du groupe.

Ses compétences de mêlée exigent une arme de mêlée compatible. Ses compétences défensives peuvent fonctionner sans bouclier, mais certaines gagnent un effet supplémentaire lorsqu'un bouclier est équipé.

## 2.1. Arbre Blindage de siège

### Identité

Arbre de tanking, de contrôle rapproché et de protection de groupe. Il attire les ennemis, accumule de la **Garde**, puis convertit cette pression en onde de choc, réparation ou protection.

```text
Ancrage d'abattoir ─► Treuil gravifique ─► Purge cinétique ─► Dôme de ferraille
Plaques de récupération ─► Réacteur de représailles ─► Dernier blindage ─► Masse inamovible
```

### 1. Ancrage d'abattoir

- **Type :** actif, centré sur soi — niveau 1.
- **Coût / recharge :** 18 Énergie ; 18 s.
- **Effet :** verrouille les vérins de l'armure pendant 6 s. L'Enforcer subit **14 % → 33 %** de dégâts en moins, résiste à **40 % → 100 %** des projections et voit sa pénalité de déplacement diminuer de **20 % → 5 %**.
- Les dégâts directs reçus à courte portée génèrent 1 charge de Garde, au maximum une fois toutes les 0,6 s.
- Avec un bouclier équipé, l'angle de blocage est élargi pendant l'Ancrage sans modifier la précision des attaques.
- **Synergies :** chaque point investi dans **Plaques de récupération** augmente sa durée de 1,5 %. **Ordre : Tenez la ligne** transmet 25 % de la réduction de l'Ancrage aux alliés très proches.

### 2. Plaques de récupération

- **Type :** passif — niveau 1.
- **Effet :** augmente l'efficacité d'armure de **1,25 % par rang** et le bouclier maximal de **0,5 % par rang**, soit +25 % d'armure et +10 % de bouclier au rang 20.
- Sous 50 % de vie, les plaques se contractent et accordent en plus **1 % → 10 %** d'efficacité d'armure.
- Les soins, réparations et restaurations de bouclier reçus pendant l'Ancrage sont augmentés de **2 % → 20 %**.
- **Synergies :** renforce directement la durée d'**Ancrage d'abattoir** et la réduction de dégâts de **Dôme de ferraille**.

### 3. Treuil gravifique

- **Type :** actif, zone pointée au curseur — niveau 6 ; requiert Ancrage d'abattoir.
- **Coût / recharge :** 14 Énergie ; **12 s → 7 s**.
- **Effet :** projette un câble à masse gravifique vers la zone visée. La portée passe de **10 m à 18 m**, le rayon de **1 m à 4 m** et les dégâts de **80 % à 180 %** des dégâts de l'arme de mêlée.
- Les ennemis légers sont tirés jusqu'à l'Enforcer ; les ennemis lourds sont rapprochés partiellement et interrompus. Les boss ne sont pas déplacés, mais subissent une forte impulsion de stagger.
- Sous Ancrage, le Treuil ne déplace jamais l'Enforcer et génère 1 Garde par ennemi capturé, dans la limite de 5.
- **Synergies :** chaque point dans **Purge cinétique** augmente les dégâts du Treuil de 1 %. Les ennemis tirés subissent **+2 % de dégâts de Purge cinétique par point investi dans Treuil gravifique** pendant 3 s.

### 4. Réacteur de représailles

- **Type :** passif — niveau 12 ; requiert Plaques de récupération.
- **Effet :** la Garde possède initialement 5 charges maximales ; ce passif ajoute **1 charge tous les 2 rangs**, jusqu'à 15 charges.
- Chaque charge confère **0,15 % → 0,4 %** de réduction de dégâts et **2 %** de puissance supplémentaire à la prochaine Purge cinétique.
- La durée avant décroissance des charges passe de **8 s à 20 s**.
- Un blocage réussi avec un bouclier produit toujours 1 Garde ; un impact reçu sans bouclier ne peut en produire qu'une fois toutes les 0,8 s.
- **Synergies :** chaque point investi augmente de 1,5 % la restitution de bouclier de **Masse inamovible** et de 1 % les dégâts de **Purge cinétique**.

### 5. Purge cinétique

- **Type :** actif, zone autour de soi — niveau 18 ; requiert Treuil gravifique.
- **Coût / recharge :** 22 Énergie ; 10 s.
- **Effet :** décharge la pression des vérins dans une onde circulaire infligeant **150 % → 340 %** des dégâts de l'arme dans un rayon de **5 m → 7 m** et provoquant un puissant stagger.
- Consomme toutes les charges de Garde. Chaque charge ajoute **6 %** de dégâts et **2 %** de rayon, avec un plafond de +90 % de dégâts et +30 % de rayon.
- Les ennemis attirés par Treuil gravifique au cours des 3 dernières secondes subissent un second impact de 35 % des dégâts.
- **Synergies :** +2 % de dégâts par point investi dans **Treuil gravifique** contre les ennemis déplacés ; +1 % de dégâts par point dans **Réacteur de représailles**. Sous **Ordre : Tenez la ligne**, la Purge accorde aux alliés proches un petit bouclier égal à 10 % des dégâts réellement infligés, plafonné par cible.

### 6. Dernier blindage

- **Type :** passif — niveau 24 ; requiert Réacteur de représailles.
- **Effet :** lorsque la vie passe sous 30 %, les plaques se referment pendant 4 s et accordent **10 % → 28 %** de réduction de dégâts. Recharge interne : **30 s → 16 s**.
- Pendant cet état, la génération de Garde est doublée et aucune charge ne décroît.
- À partir du rang 10, l'activation provoque une impulsion de provocation sur les ennemis normaux proches ; les boss ne sont pas forcés, mais leur priorité de menace envers l'Enforcer augmente en coopération.
- **Synergies :** chaque point dans **Présence de fer** augmente la durée de 1 %. Une Purge cinétique lancée pendant Dernier blindage restaure **0,1 % de vie par charge de Garde et par tranche de 5 rangs**.

### 7. Dôme de ferraille

- **Type :** actif, aura mobile autour de soi — niveau 30 ; requiert Purge cinétique.
- **Coût / recharge :** 30 Énergie ; **30 s → 22 s**.
- **Effet :** déploie pendant **6 s → 10 s** un champ défensif de **5 m → 8 m**. Les joueurs à l'intérieur subissent **8 % → 20 %** de dégâts en moins et bénéficient d'une réduction supplémentaire de 10 % contre projectiles et explosions.
- Les projectiles traversant la limite du dôme produisent un impact visuel industriel et perdent une partie de leur puissance ; le dôme ne bloque pas la visibilité.
- Sous Ancrage d'abattoir, le rayon augmente de 25 % et le Dôme ne peut être interrompu par une projection.
- **Synergies :** chaque point dans **Plaques de récupération** ajoute 0,25 point de pourcentage à sa réduction principale, plafonnée à +5 %. **Présence de fer** augmente la résistance aux contrôles des alliés situés dans le Dôme.

### 8. Masse inamovible

- **Type :** passif — niveau 30 ; requiert Dernier blindage.
- **Effet :** lorsque l'Enforcer se trouve à moins de 8 m d'un boss ou d'au moins trois ennemis, il gagne **5 % → 18 %** de réduction de dégâts et **25 % → 150 %** de génération de menace en coopération.
- La Garde ne décroît plus tant que la condition est remplie.
- Chaque groupe de 5 charges de Garde consommées restaure **0,5 % → 2,5 %** du bouclier maximal ; sans bouclier actif, la moitié de cette valeur restaure la vie.
- **Synergies :** chaque point dans **Dôme de ferraille** augmente cette restauration de 1 %. **Autorité de massacre** permet aux activations d'Ordre de produire immédiatement une partie des charges nécessaires.

## 2.2. Arbre Assaut vélocitaire

### Identité

Arbre de mêlée rapide, de déplacement agressif et d'exécution. Les impacts successifs produisent du **Carnage**, une ressource temporaire qui accélère les servomoteurs et alimente les attaques de finition.

```text
Percée d'équarrissage ─► Lacération croisée ─► Exécution hydraulique ─► Rotor de boucherie
Servomoteurs agressifs ─► Moteur de carnage ─► Réflexe d'équarrissage ─► Surcadence prédatrice
```

### 1. Percée d'équarrissage

- **Type :** actif, ruée en ligne — niveau 1.
- **Coût / recharge :** 12 Énergie ; **7 s → 4,5 s**.
- **Effet :** traverse rapidement **8 m → 13 m** et frappe tous les ennemis sur le trajet pour **90 % → 230 %** des dégâts de l'arme de mêlée.
- Chaque ennemi touché génère 1 Carnage, au maximum 3 par utilisation.
- La ruée peut être dirigée dans les airs sur une courte distance, mais ne remplace pas le double saut.
- **Synergies :** +1,5 % de dégâts par point dans **Servomoteurs agressifs**. Sous **Ordre : Avancez**, la distance augmente de 20 % et la récupération après la ruée est supprimée.

### 2. Servomoteurs agressifs

- **Type :** passif — niveau 1.
- **Effet :** les impacts de mêlée espacés de moins de 2 s produisent du Carnage. Le maximum passe de **3 charges au rang 1 à 10 charges au rang 20**.
- Chaque charge accorde **0,5 % → 1,5 %** de vitesse d'attaque de mêlée et **0,4 % → 1 %** de vitesse de déplacement.
- Après 3 s sans impact, une charge disparaît toutes les 1,5 s. Une élimination de mêlée rafraîchit la durée de toutes les charges.
- **Synergies :** chaque point augmente de 1 % les dégâts de **Percée d'équarrissage** et de 0,5 % la vitesse d'animation de **Lacération croisée**, sans modifier la précision.

### 3. Lacération croisée

- **Type :** actif, monocible avec petit arc frontal — niveau 6 ; requiert Percée d'équarrissage.
- **Coût / recharge :** 16 Énergie ; **6 s → 4 s**.
- **Effet :** effectue deux frappes rapides de **70 % → 150 %** des dégâts de l'arme chacune. En combat à deux armes, une arme différente exécute chaque frappe ; avec une seule arme, la seconde frappe conserve 80 % de sa puissance.
- Chaque frappe applique une charge de **Trauma**, jusqu'à 5. Un Trauma inflige un faible saignement industriel pendant 6 s et sert surtout de charge d'exécution.
- Chaque charge de Carnage augmente les dégâts de Lacération de 2 %.
- **Synergies :** +2 % de dégâts par point dans **Percée d'équarrissage**. Chaque point dans **Moteur de carnage** augmente de 1 % la durée du Trauma.

### 4. Moteur de carnage

- **Type :** passif — niveau 12 ; requiert Servomoteurs agressifs.
- **Effet :** chaque charge de Carnage augmente la récupération des compétences actives de mêlée de **0,25 % → 1,25 %**.
- Une élimination de mêlée restaure **1 → 5 Énergie**, rafraîchit le Carnage et empêche sa décroissance pendant 2 s.
- À partir du rang 10, atteindre le maximum de Carnage réduit de 20 % le coût de la prochaine compétence de mêlée.
- **Synergies :** renforce directement la boucle de **Percée**, **Lacération**, **Exécution** et **Rotor**. Les éliminations de cibles sous **Marquage d'extermination** restaurent deux fois plus d'Énergie.

### 5. Exécution hydraulique

- **Type :** actif, monocible — niveau 18 ; requiert Lacération croisée.
- **Coût / recharge :** 24 Énergie ; 10 s.
- **Effet :** concentre les vérins dans un coup terminal infligeant **220 % → 520 %** des dégâts de l'arme.
- Consomme jusqu'à 5 Trauma et jusqu'à 10 Carnage : chaque Trauma ajoute 15 % de dégâts et chaque Carnage 5 %.
- Contre une cible sous 25 % de vie, le coup reçoit encore +50 % de dégâts, sans exécuter automatiquement les boss.
- Si la cible meurt, la moitié du Carnage consommé est rendue et 50 % du temps de recharge est récupéré.
- **Synergies :** +2 % de dégâts de base par point dans **Lacération croisée**. Chaque point dans **Réflexe d'équarrissage** augmente de 1 % le remboursement de recharge après élimination.

### 6. Réflexe d'équarrissage

- **Type :** passif — niveau 24 ; requiert Moteur de carnage.
- **Effet :** une élimination de mêlée restaure immédiatement une partie de la recharge de Percée d'équarrissage et rend la prochaine compétence de mêlée utilisée dans les 4 s **30 % → 100 %** moins coûteuse en Énergie.
- Recharge interne : **8 s → 3 s** ; les élites et boss déclenchent toujours l'effet lorsqu'ils meurent.
- À partir du rang 15, une élimination obtenue avec Purge cinétique peut également déclencher le réflexe.
- **Synergies :** chaque point réduit de 0,15 s la recharge interne et augmente de 1 % les dégâts de la prochaine Percée gratuite.

### 7. Rotor de boucherie

- **Type :** actif, zone mobile autour de soi — niveau 30 ; requiert Exécution hydraulique.
- **Coût / recharge :** 32 Énergie ; 12 s.
- **Effet :** fait tourner l'arme pendant environ 1,2 s tout en autorisant le déplacement. Produit **4 → 7 impacts** de **45 % → 75 %** des dégâts de l'arme dans un rayon de **5 m → 7 m**.
- Les impacts au-delà du quatrième consomment chacun 1 Carnage. Sans Carnage disponible, le Rotor s'arrête après le quatrième impact.
- Les ennemis récemment tirés par Treuil gravifique ou porteurs de Trauma subissent +25 % de dégâts.
- **Synergies :** +2 % de dégâts par point dans **Percée d'équarrissage** ; +1 % de rayon par tranche de 5 points dans **Lacération croisée**. Sous **Ordre : Avancez**, le Rotor n'impose aucune pénalité de déplacement.

### 8. Surcadence prédatrice

- **Type :** passif — niveau 30 ; requiert Réflexe d'équarrissage.
- **Effet :** au maximum de Carnage, chaque quatrième impact de mêlée déclenche un second mouvement mécanique fantôme infligeant **25 % → 75 %** des dégâts du coup principal à la cible et 50 % de cette valeur dans un petit cône arrière.
- Le Carnage ne décroît plus pendant le sprint, la glissade ou une Percée.
- Chaque groupe de 5 charges de Garde consommées accorde 1 Carnage, ce qui relie les arbres offensif et défensif.
- **Synergies :** +1 % de dégâts de l'impact fantôme par point dans **Exécution hydraulique**. **Autorité de massacre** peut générer immédiatement du Carnage à l'activation d'un Ordre.

## 2.3. Arbre Doctrine de commandement

### Identité

Arbre de buffs, de soutien coopératif et de coordination. Un seul **Ordre** peut être actif à la fois : activer un nouvel Ordre remplace le précédent. Cette limitation transforme les Ordres en choix tactiques et évite l'empilement permanent de bonus.

```text
Ordre : Avancez ─► Ordre : Tenez la ligne ─► Marquage d'extermination ─► Ralliement traumatique
Présence de fer ─► Chaîne de commandement ─► Doctrine du survivant ─► Autorité de massacre
```

### 1. Ordre : Avancez

- **Type :** actif, buff de groupe autour de soi — niveau 1.
- **Coût / recharge :** 18 Énergie ; 18 s.
- **Effet :** pendant **8 s → 12 s**, les alliés dans un rayon de 12 m gagnent **8 % → 24 %** de vitesse de déplacement et **4 % → 14 %** de vitesse d'action : attaques, rechargements et préparations courtes de compétences.
- L'Ordre reste appliqué 2 s après avoir quitté la zone.
- N'augmente ni la précision, ni l'assistance de visée, ni la stabilité du réticule.
- **Synergies :** chaque point dans **Présence de fer** augmente sa puissance de 0,5 %. Percée d'équarrissage et Rotor de boucherie reçoivent leurs bonus spécifiques pendant l'Ordre.

### 2. Présence de fer

- **Type :** passif, aura — niveau 1.
- **Effet :** l'Enforcer et les alliés à moins de 10 m gagnent **8 % → 50 %** de résistance au stagger et **2 % → 20 %** de réduction de durée des ralentissements, immobilisations et effets de peur.
- L'Enforcer bénéficie toujours de l'effet complet, même seul.
- Le compagnon principal de l'Engineer reçoit 75 % de l'effet ; les drones temporaires 40 %.
- **Synergies :** augmente la puissance d'**Ordre : Avancez**, la durée de **Dernier blindage** et la résistance aux contrôles dans **Dôme de ferraille**.

### 3. Ordre : Tenez la ligne

- **Type :** actif, buff de groupe autour de soi — niveau 6 ; requiert Ordre : Avancez.
- **Coût / recharge :** 18 Énergie ; 18 s.
- **Effet :** pendant **8 s → 12 s**, les alliés gagnent **10 % → 30 %** d'efficacité d'armure et de résistances, réduisent leur délai de recharge de bouclier de **5 % → 35 %** et résistent à **20 % → 100 %** des projections.
- Remplace immédiatement Ordre : Avancez s'il est actif.
- **Synergies :** chaque point dans **Plaques de récupération** ajoute 0,4 % d'efficacité au bonus défensif. Sous Ancrage, 25 % de la réduction personnelle de l'Enforcer est transmise aux alliés situés à moins de 4 m.

### 4. Chaîne de commandement

- **Type :** passif — niveau 12 ; requiert Présence de fer.
- **Effet :** augmente la puissance personnelle des Ordres de **0,5 % par rang**, soit +10 % au rang 20.
- Chaque autre joueur allié présent dans l'aura ajoute **1 % → 4 %** de puissance d'Ordre, jusqu'à quatre alliés. Les compagnons et drones ne comptent pas comme joueurs pour ce bonus.
- La persistance d'un Ordre après sortie de l'aura passe de **1 s à 4 s**.
- **Synergies :** améliore tous les Ordres et augmente de 1 % par point la durée du bonus obtenu après la mort d'une cible sous Marquage d'extermination.

### 5. Marquage d'extermination

- **Type :** actif, monocible — niveau 18 ; requiert Ordre : Tenez la ligne.
- **Coût / recharge :** 22 Énergie ; **16 s → 12 s**.
- **Effet :** marque une cible pendant **6 s → 12 s**. Elle subit **8 % → 22 %** de dégâts supplémentaires de toutes les sources alliées.
- À sa mort, elle émet une impulsion accordant pendant 3 s **10 % → 25 %** de vitesse de déplacement et de rechargement aux alliés proches. L'Enforcer reçoit également 3 Garde et 3 Carnage.
- Sur un boss, chaque tranche de 5 % de vie perdue pendant le marquage déclenche une impulsion réduite, une seule fois par tranche.
- **Synergies :** Treuil gravifique attire plus fortement les cibles marquées ; Exécution hydraulique double son remboursement si elle tue la cible. Chaque point dans **Doctrine du survivant** augmente la durée du marquage de 0,5 %.

### 6. Doctrine du survivant

- **Type :** passif — niveau 24 ; requiert Chaîne de commandement.
- **Effet :** une élimination réalisée par un allié sous l'effet d'un Ordre prolonge cet Ordre de **0,15 s → 0,5 s**, jusqu'à 4 s supplémentaires par activation. Une élite compte pour quatre éliminations.
- L'allié récupère également **0,25 % → 1,5 %** de son bouclier maximal ; sans bouclier actif, la moitié est rendue en vie.
- Sur les boss, retirer 5 % de leur vie pendant un Ordre prolonge celui-ci de 0,5 s, dans la même limite globale.
- **Synergies :** chaque point augmente de 1 % les restaurations de **Ralliement traumatique** et de 0,5 % l'impulsion produite par une cible marquée.

### 7. Ralliement traumatique

- **Type :** actif, zone autour de soi — niveau 30 ; requiert Marquage d'extermination.
- **Coût / recharge :** 36 Énergie ; **36 s → 24 s**.
- **Effet :** purge ralentissements, immobilisations, peur et stagger sur les alliés proches. Restaure **10 % → 35 %** de leur bouclier et **5 → 25 Énergie**.
- Un allié sans bouclier actif récupère à la place **5 % → 15 %** de sa vie maximale.
- Accorde **0,5 s → 1,5 s** d'immunité aux contrôles déjà purgés. Le compagnon principal reçoit 75 % des restaurations ; les drones temporaires ne sont pas soignés par cette compétence.
- **Synergies :** +1 % de restauration par point dans **Doctrine du survivant**. Si Dernier blindage est actif, l'Enforcer reçoit 50 % de restauration supplémentaire sans la transmettre aux autres.

### 8. Autorité de massacre

- **Type :** passif — niveau 30 ; requiert Doctrine du survivant.
- **Effet :** activer un Ordre produit immédiatement **1 → 5 Garde** et **1 → 5 Carnage** pour l'Enforcer.
- Tant qu'un Ordre est actif :
  - une compétence de Blindage de siège utilisée accorde aux alliés un petit bouclier égal à **2 % → 8 %** du bouclier maximal ;
  - une compétence d'Assaut vélocitaire utilisée accorde **2 % → 8 %** de vitesse d'action pendant 2 s.
- Chaque effet possède une recharge interne de 3 s afin d'éviter le spam.
- Au rang 20, Ralliement traumatique peut être lancé sans remplacer l'Ordre actif et prolonge celui-ci de 2 s.
- **Synergies :** constitue le pont principal entre les trois arbres : Ordres vers Garde/Carnage, Défense vers bouclier de groupe et Mêlée vers accélération de groupe.

## 2.4. Synergies structurantes de l'Enforcer

1. **Treuil gravifique → Purge cinétique / Rotor de boucherie** : rassemble les ennemis avant les zones de mêlée.
2. **Ancrage d'abattoir → Réacteur de représailles → Purge cinétique** : transforme les coups reçus en contre-attaque de zone.
3. **Lacération croisée → Exécution hydraulique** : accumule Trauma puis le consume sur une cible prioritaire.
4. **Ordre : Avancez → Percée / Rotor** : supprime les pénalités de déplacement et accélère le rythme offensif.
5. **Ordre : Tenez la ligne → Ancrage / Dôme** : construit un véritable rempart coopératif.
6. **Marquage d'extermination → Exécution hydraulique** : boucle de chasse aux élites et de remboursement des ressources.
7. **Autorité de massacre** : convertit les Ordres en ressources personnelles et les compétences personnelles en micro-buffs de groupe.

---
# 3. Marine

Le Marine transforme ses armes conventionnelles en trois mécaniques distinctes :

- **Saturation** pour les impacts successifs des armes légères ;
- **Fracture** pour les coups massifs des armes lourdes ;
- **Amorce** pour les réactions en chaîne explosives.

Les compétences ne rendent jamais les tirs plus précis. Elles augmentent la cadence, la mobilité, la pénétration, l'impact, les réactions de cible ou la violence des munitions réellement touchées.

## 3.1. Arbre Doctrine de saturation

### Identité

Arbre des pistolets, mitraillettes et armes automatiques légères à une main. Il récompense les séries d'impacts, le combat mobile, le rechargement agressif et l'utilisation éventuelle de deux armes.

```text
Rafale terminale ─► Rechargement de combat ─► Double détente ─► Tempête de culasse
Cadence prédatrice ─► Alimentation brutale ─► Pluie de douilles ─► Chasseur sous adrénaline
```

### 1. Rafale terminale

- **Type :** actif, tir vers le réticule — niveau 1.
- **Compatibilité :** armes légères à une main.
- **Coût / recharge :** 10 Énergie ; **8 s → 5 s** ; consomme les munitions normalement.
- **Effet :** déclenche immédiatement **3 → 7 tirs**. Chaque projectile inflige **70 % → 100 %** des dégâts normaux de l'arme et génère de la Saturation s'il touche.
- Avec deux armes légères, les tirs alternent entre les deux mains. Avec une seule arme, toute la rafale vient de cette arme.
- Le Marine conserve son déplacement normal pendant l'activation.
- **Synergies :** +1,5 % de dégâts par point dans **Cadence prédatrice**. **Pluie de douilles** peut se déclencher plusieurs fois pendant la rafale si son compteur atteint le seuil.

### 2. Cadence prédatrice

- **Type :** passif — niveau 1.
- **Effet :** les impacts successifs d'armes légères espacés de moins de 1,5 s produisent de la Saturation. Le maximum passe de **4 charges au rang 1 à 12 charges au rang 20**.
- Chaque charge accorde **0,5 % → 1,2 %** de cadence de tir et **0,5 % → 1 %** de vitesse de rechargement.
- Après 2 s sans impact, une charge disparaît toutes les secondes. Recharger n'efface pas automatiquement la Saturation, mais sans Alimentation brutale une moitié des charges est perdue.
- **Synergies :** chaque point augmente de 1 % les dégâts de **Rafale terminale** et de 0,5 % la distance de **Rechargement de combat**.

### 3. Rechargement de combat

- **Type :** actif, déplacement instantané — niveau 6 ; requiert Rafale terminale.
- **Coût / recharge :** 12 Énergie ; **9 s → 6 s**.
- **Effet :** effectue un pas brutal de **3 m → 6 m** dans la direction de déplacement et recharge instantanément **15 % → 60 %** du chargeur de chaque arme légère équipée, en prélevant les munitions dans la réserve.
- Conserve toutes les charges de Saturation pendant 2 s, même sans toucher.
- Déclenché durant une glissade, le déplacement gagne 30 % de distance et peut franchir de petits obstacles bas.
- **Synergies :** chaque point dans **Alimentation brutale** ajoute 1 % de remplissage de chargeur. Sous **Ordre : Avancez**, la recharge de la compétence est réduite de 15 %.

### 4. Alimentation brutale

- **Type :** passif — niveau 12 ; requiert Cadence prédatrice.
- **Effet :** lors d'un rechargement avec au moins la moitié de la Saturation maximale, conserve **20 % → 100 %** des charges.
- Les **3 → 8 premiers tirs** suivant le rechargement infligent **5 % → 25 %** de dégâts supplémentaires.
- À partir du rang 10, ces tirs traversent un ennemi léger supplémentaire ; ils ne gagnent aucune correction de trajectoire.
- Une recharge complète effectuée par Rechargement de combat compte comme un rechargement pour ce passif.
- **Synergies :** chaque point augmente de 1 % le remplissage de **Rechargement de combat** et de 1 % les dégâts de la prochaine **Double détente** lancée dans les 3 s.

### 5. Double détente

- **Type :** actif, monocible au réticule — niveau 18 ; requiert Rechargement de combat.
- **Coût / recharge :** 22 Énergie ; 12 s ; consomme les munitions normalement.
- **Effet :** vide une courte salve coordonnée des deux mains sur la cible visée pour un total de **250 % → 650 %** des dégâts d'arme.
- Avec une seule arme légère, la compétence produit deux rafales successives avec un très court délai.
- Consomme jusqu'à 6 charges de Saturation ; chaque charge consommée ajoute 8 % de dégâts.
- Entre 0 et 5 m, les dégâts augmentent progressivement jusqu'à +40 %, ce qui en fait une exécution de proximité.
- **Synergies :** +2 % de dégâts de base par point dans **Rafale terminale**. Une cible porteuse de **Criblé** subit un éclatement immédiat à la fin de la compétence.

### 6. Pluie de douilles

- **Type :** passif — niveau 24 ; requiert Alimentation brutale.
- **Effet :** lorsque le Marine possède au moins 6 Saturation, chaque **12e → 5e impact** d'arme légère déclenche une gerbe balistique latérale infligeant **25 % → 70 %** des dégâts de l'arme dans un petit cône autour de la cible.
- La gerbe peut toucher plusieurs ennemis mais ne peut pas retoucher la cible principale.
- Une élimination causée par cette gerbe rend 1 munition à chaque arme légère équipée, dans la limite de sa réserve maximale.
- **Synergies :** chaque point dans **Alimentation brutale** augmente la largeur du cône de 1 %. Les fragments gagnent les effets de **Shrapnel industriel** si cette compétence est apprise.

### 7. Tempête de culasse

- **Type :** actif, buff personnel — niveau 30 ; requiert Double détente.
- **Coût / recharge :** 34 Énergie ; **30 s → 22 s**.
- **Effet :** pendant **4 s → 8 s**, les armes légères s'alimentent directement depuis la réserve sans rechargement et gagnent **10 % → 30 %** de cadence de tir ainsi que 10 % de vitesse de déplacement.
- Les munitions sont toujours réellement consommées. Aucun projectile supplémentaire n'est créé gratuitement.
- Chaque élimination prolonge l'effet de 0,25 s, jusqu'à 3 s supplémentaires par activation.
- **Synergies :** chaque point dans **Chasseur sous adrénaline** augmente la durée de 0,5 %. Atteindre la Saturation maximale pendant l'effet applique immédiatement Criblé à la cible actuelle.

### 8. Chasseur sous adrénaline

- **Type :** passif — niveau 30 ; requiert Pluie de douilles.
- **Effet :** au maximum de Saturation, les impacts d'armes légères appliquent **Criblé**. Au dixième impact sur une même cible, les projectiles logés éclatent et infligent **50 % → 180 %** de dégâts balistiques dans un rayon de 3 m.
- Une explosion du Marine peut détoner Criblé avant dix impacts ; elle inflige alors une proportion des dégâts correspondant au nombre de charges présentes.
- Une élimination par éclatement réduit de **0,5 s → 2 s** la recharge de Rechargement de combat.
- **Synergies :** +1 % de dégâts d'éclatement par point dans **Double détente**. **Réaction en chaîne** traite un Criblé complet comme une charge d'Amorce supplémentaire.

## 3.2. Arbre Ordnance de rupture

### Identité

Arbre des fusils lourds, canons et armes de tir à deux mains. Il utilise des tirs plus lents pour traverser les lignes ennemies, provoquer du stagger et accumuler de la **Fracture** jusqu'à rompre l'armure.

```text
Percuteur surchargé ─► Ligne de démolition ─► Impact sismique ─► Surcharge de canon
Munitions à noyau dense ─► Masse d'arrêt ─► Chambre sacrificielle ─► Architecture de siège
```

### 1. Percuteur surchargé

- **Type :** actif, amélioration du prochain tir — niveau 1.
- **Compatibilité :** armes à distance à deux mains.
- **Coût / recharge :** 12 Énergie ; **8 s → 5 s**.
- **Effet :** le prochain tir effectué dans les 4 s inflige **150 % → 360 %** des dégâts normaux, consomme une munition supplémentaire et produit un puissant recul sur la cible.
- Si l'arme tire déjà un projectile explosif, le bonus augmente l'impact principal mais ne double pas arbitrairement tous les sous-projectiles.
- Le tir applique une charge supplémentaire de Fracture.
- **Synergies :** +1,5 % de dégâts par point dans **Munitions à noyau dense**. Tirer sur une cible sous Armure rompue réduit la recharge de 20 %.

### 2. Munitions à noyau dense

- **Type :** passif — niveau 1.
- **Effet :** les tirs d'armes lourdes ignorent **2 % → 25 %** de l'armure et infligent **5 % → 50 %** de stagger supplémentaire.
- Aux rangs 8 et 16, les projectiles non explosifs gagnent chacun une traversée supplémentaire, jusqu'à deux ennemis additionnels.
- La pénétration ne modifie ni la dispersion, ni le recul de l'arme, ni la précision du joueur.
- **Synergies :** augmente les dégâts de **Percuteur surchargé** et le rayon de stagger d'**Impact sismique**.

### 3. Ligne de démolition

- **Type :** actif, tir traversant en ligne — niveau 6 ; requiert Percuteur surchargé.
- **Coût / recharge :** 18 Énergie ; **10 s → 7 s** ; consomme les munitions de l'arme.
- **Effet :** déclenche un tir de siège infligeant **120 % → 300 %** des dégâts de l'arme à tous les ennemis alignés jusqu'à **25 m → 40 m**.
- Le projectile perd 10 % de dégâts par cible traversée, sans descendre sous 60 % de sa valeur initiale.
- Chaque cible touchée reçoit une charge de Fracture.
- **Synergies :** +2 % de dégâts par point dans **Masse d'arrêt**. Les ennemis déjà sous Armure rompue ne réduisent pas les dégâts transmis à la cible suivante.

### 4. Masse d'arrêt

- **Type :** passif — niveau 12 ; requiert Munitions à noyau dense.
- **Effet :** les impacts d'armes lourdes appliquent Fracture, jusqu'à 5 charges. Chaque charge augmente les dégâts lourds reçus de **1 % → 3 %** et le stagger reçu de **5 % → 15 %**.
- À 5 charges, la cible passe en **Armure rompue** pendant **4 s → 10 s** : son armure est réduite de **10 % → 25 %** et ses charges de Fracture sont consommées.
- Les boss ne sont pas automatiquement interrompus, mais conservent la réduction d'armure et de résistance au stagger.
- **Synergies :** chaque point augmente de 1 % les dégâts de **Ligne de démolition**. **Shrapnel industriel** profite pleinement de la réduction d'armure.

### 5. Impact sismique

- **Type :** actif, explosion au point d'impact — niveau 18 ; requiert Ligne de démolition.
- **Coût / recharge :** 24 Énergie ; **14 s → 10 s** ; consomme les munitions de l'arme.
- **Effet :** tire un projectile de choc qui éclate sur la première surface ou cible rencontrée et inflige **180 % → 420 %** des dégâts de l'arme dans un rayon de **4 m → 7 m**.
- Les ennemis sont repoussés depuis le centre et reçoivent 2 Fracture.
- Une cible impossible à déplacer reçoit à la place 50 % de stagger supplémentaire.
- **Synergies :** +2 % de dégâts par point dans **Munitions à noyau dense**. Si l'explosion touche une Mine charognarde, celle-ci détonne immédiatement avec +20 % de rayon.

### 6. Chambre sacrificielle

- **Type :** passif — niveau 24 ; requiert Masse d'arrêt.
- **Effet :** lorsque le chargeur d'une arme lourde passe sous 25 %, ses tirs infligent **8 % → 30 %** de dégâts supplémentaires et leurs projectiles gagnent **10 % → 50 %** de vitesse.
- Tuer un ennemi avec la dernière munition du chargeur recharge immédiatement **15 % → 60 %** de celui-ci depuis la réserve.
- Si aucun rechargement n'est possible, la prochaine compétence lourde coûte **10 % → 40 %** d'Énergie en moins.
- **Synergies :** chaque point augmente de 1 % les dégâts de **Surcharge de canon** lorsqu'elle consomme les dernières munitions du chargeur.

### 7. Surcharge de canon

- **Type :** actif, tir majeur monocible avec petite zone — niveau 30 ; requiert Impact sismique.
- **Coût / recharge :** 38 Énergie ; **20 s → 14 s** ; consomme trois munitions.
- **Effet :** après une préparation mobile de 0,65 s, libère un tir infligeant **500 % → 1 050 %** des dégâts de l'arme à la cible principale et 60 % dans un rayon de **3 m → 6 m**.
- Applique immédiatement 5 Fracture ; si la cible était déjà sous Armure rompue, elle conserve l'état et subit +25 % de dégâts.
- Le Marine reste capable de marcher pendant la préparation mais ne peut ni sprinter ni glisser.
- **Synergies :** +2 % de dégâts par point dans **Ligne de démolition** et +1 % par point dans **Chambre sacrificielle** lorsque la compétence vide le chargeur. Détonne toutes les Amorce de la cible.

### 8. Architecture de siège

- **Type :** passif — niveau 30 ; requiert Chambre sacrificielle.
- **Effet :** toucher une cible sous Armure rompue réduit les recharges des compétences lourdes de **0,05 s → 0,25 s**, avec un maximum de 2 s récupérées par seconde.
- **10 % → 50 %** des dégâts excédentaires d'une élimination lourde poursuivent leur trajectoire sous forme d'une onde étroite derrière la cible.
- Les compétences lourdes détonnent les Amorce et infligent +20 % de dégâts aux ennemis Déchiquetés.
- **Synergies :** +1 % de dégâts d'onde par point dans **Surcharge de canon**. Constitue le pont principal entre armes lourdes et explosifs.

## 3.3. Arbre Dévastation contrôlée

### Identité

Arbre des grenades, mines, roquettes et charges de proximité. Il couvre le terrain, prépare des **Amorce**, puis provoque des réactions en chaîne. Les explosions sont placées au curseur ou déployées autour du Marine.

```text
Grenade M-13 « Écorcheuse » ─► Mine charognarde ─► Roquette thermobarique ─► Couronne de charges
Ceinture de démolition ─► Shrapnel industriel ─► Réaction en chaîne ─► Protocole Terre brûlée
```

### 1. Grenade M-13 « Écorcheuse »

- **Type :** actif, projectile en cloche vers le curseur — niveau 1.
- **Charges / recharge :** 2 charges ; **10 s → 7 s** par charge ; 14 Énergie.
- **Effet :** explose après impact ou court délai et inflige **150 % → 360 %** de dégâts explosifs dans un rayon de **4 m → 6,5 m**.
- Produit une importante réaction gore contre les cibles biologiques et projette des composants sur les machines.
- Applique une Amorce si Réaction en chaîne est apprise.
- **Synergies :** +1,5 % de dégâts par point dans **Ceinture de démolition**. Les fragments profitent de **Shrapnel industriel**.

### 2. Ceinture de démolition

- **Type :** passif — niveau 1.
- **Effet :** augmente le rayon de toutes les explosions du Marine de **0,75 % par rang**, soit +15 % au rang 20, et réduit de **20 % → 80 %** les dégâts qu'il subit de ses propres explosifs lorsque l'option de dégâts personnels est active.
- Aux rangs 10 et 20, Grenade M-13 et Mine charognarde gagnent chacune une charge maximale supplémentaire.
- Ne réduit pas les dégâts reçus des explosifs ennemis.
- **Synergies :** augmente les dégâts de Grenade M-13 et le nombre de charges projetées par **Couronne de charges**.

### 3. Mine charognarde

- **Type :** actif, dispositif placé au curseur — niveau 6 ; requiert Grenade M-13.
- **Charges / recharge :** **2 → 6 mines actives simultanément** ; 8 s par charge ; 16 Énergie.
- **Effet :** pose une mine robotisée au sol. Elle explose à proximité d'un ennemi ou lors d'une nouvelle activation maintenue de la compétence, pour **170 % → 400 %** de dégâts dans un rayon de **3,5 m → 5,5 m**.
- Les mines persistent 30 s. Poser une mine au-delà du maximum détruit la plus ancienne sans explosion.
- Une mine frappée par Impact sismique ou une autre explosion du Marine détonne immédiatement.
- **Synergies :** +2 % de dégâts par point dans **Réaction en chaîne**. Chaque point dans **Ceinture de démolition** augmente sa durée de 1 %.

### 4. Shrapnel industriel

- **Type :** passif — niveau 12 ; requiert Ceinture de démolition.
- **Effet :** les explosions appliquent **Déchiqueté** pendant **4 s → 10 s**. La cible subit **2 % → 15 %** de dégâts physiques supplémentaires et un léger saignement si elle possède une anatomie compatible.
- Une cible tuée par une explosion libère trois fragments infligeant chacun **20 % → 80 %** des dégâts explosifs initiaux à un ennemi proche.
- Les machines libèrent des éclats métalliques plutôt qu'un effet organique, mais la mécanique reste identique.
- **Synergies :** les gerbes de **Pluie de douilles** héritent de Déchiqueté. Chaque point augmente de 1 % les dégâts de **Couronne de charges** contre les cibles déjà Déchiquetées.

### 5. Roquette thermobarique

- **Type :** actif, projectile vers le curseur — niveau 18 ; requiert Mine charognarde.
- **Charges / recharge :** 2 charges ; **14 s → 10 s** par charge ; 26 Énergie.
- **Effet :** tire une roquette lente et lisible infligeant **300 % → 720 %** de dégâts dans un rayon de **5 m → 9 m**.
- Laisse pendant **4 s → 8 s** une zone de combustion infligeant **25 % → 70 %** de puissance explosive par seconde.
- Les ennemis projetés sur une Mine charognarde la déclenchent.
- **Synergies :** +2 % de dégâts par point dans **Grenade M-13**. Les Brûlures du Scientist présentes sur les cibles augmentent de 20 % la durée de la zone thermobarique, sans empilement multiple.

### 6. Réaction en chaîne

- **Type :** passif — niveau 24 ; requiert Shrapnel industriel.
- **Effet :** les explosions appliquent une Amorce, jusqu'à 3 par cible. La mort de la cible ou l'impact d'une compétence explosive différente consomme les Amorce et déclenche une explosion secondaire de **30 % → 120 %** de puissance par charge.
- Une cible possède une recharge interne de 0,5 s entre deux réactions afin d'éviter une boucle infinie.
- Un Criblé complet compte comme une Amorce supplémentaire ; une compétence lourde peut détoner toutes les charges.
- **Synergies :** chaque point augmente de 1 % les dégâts de **Mine charognarde** et de 0,5 % le rayon des réactions secondaires.

### 7. Couronne de charges

- **Type :** actif, zone annulaire autour de soi — niveau 30 ; requiert Roquette thermobarique.
- **Coût / recharge :** 36 Énergie ; **22 s → 16 s**.
- **Effet :** projette **6 → 12 charges** autour du Marine entre 3 et 8 m. Elles explosent après 0,8 s pour **80 % → 180 %** de dégâts chacune.
- La zone centrale demeure sûre pour permettre au Marine de glisser ou bondir hors de l'encerclement. Un ennemi peut être touché par plusieurs charges, avec 35 % de dégâts pour chaque impact supplémentaire après le premier.
- Les mines existantes dans l'anneau sont également déclenchées.
- **Synergies :** +2 % de dégâts par point dans **Mine charognarde** et +1 % contre les cibles Déchiquetées par point dans **Shrapnel industriel**.

### 8. Protocole Terre brûlée

- **Type :** passif — niveau 30 ; requiert Réaction en chaîne.
- **Effet :** utiliser des compétences explosives différentes dans un intervalle de 6 s produit une charge d'**Escalade**, jusqu'à 3.
- Chaque Escalade accorde **3 % → 8 %** de dégâts explosifs et **5 % → 15 %** de puissance aux réactions secondaires.
- À 3 charges, la prochaine compétence explosive ne consomme pas de charge d'usage, déclenche toutes les Amorce présentes dans sa zone et remet l'Escalade à zéro.
- Alterner Grenade, Mine, Roquette et Couronne récupère **5 % → 25 %** de leur temps de recharge restant.
- **Synergies :** Armure rompue augmente de 25 % les fragments et réactions ; Criblé fournit une Amorce supplémentaire. Cet ultime passif récompense donc les builds mêlant les trois arbres.

## 3.4. Synergies structurantes du Marine

1. **Cadence prédatrice → Rafale terminale → Tempête de culasse** : construit puis exploite la Saturation.
2. **Rechargement de combat → Alimentation brutale** : transforme le rechargement en outil de mobilité et de maintien de pression.
3. **Criblé → Explosifs** : une cible saturée devient une bombe préparée par les armes légères.
4. **Masse d'arrêt → Armure rompue → Shrapnel industriel** : les armes lourdes ouvrent la cible aux dégâts physiques et aux fragments.
5. **Mine charognarde → Impact sismique / Couronne de charges** : pose puis déclenche un terrain explosif.
6. **Amorce → Surcharge de canon** : le tir lourd majeur fait exploser les charges accumulées.
7. **Protocole Terre brûlée** : récompense l'alternance des différentes familles explosives plutôt que la répétition d'une seule grenade.

---
# 4. Scientist

Le Scientist ne lance pas de magie médiévale. Il manipule des réacteurs portatifs, des gantelets expérimentaux, des orbes de confinement, des bobines et des phénomènes extradimensionnels mal compris.

Ses trois arbres créent des états élémentaires compatibles entre eux :

- **Brûlure** pour les dégâts persistants et la propagation ;
- **Cryostase** pour le ralentissement, le gel et la fragilisation ;
- **Conductivité** pour les chaînes électriques et les surcharges.

Les réactions entre éléments permettent des builds purs ou hybrides sans transformer les compétences en simples variantes colorées d'un même projectile.

## 4.1. Arbre Thermodynamique interdite

### Identité

Arbre au potentiel de dégâts total le plus élevé. Il applique des Brûlures, contamine le terrain, propage les dégâts et consume les effets persistants dans des détonations instantanées.

```text
Inciseur thermique ─► Nappe de prométhium ─► Flashover ─► Corona de fusion
Combustion catalytique ─► Propagation pyrolytique ─► Cœur de four noir ─► Entropie affamée
```

### 1. Inciseur thermique

- **Type :** actif, rayon monocible au réticule — niveau 1.
- **Coût / recharge :** 12 Énergie ; **4,5 s → 3 s**.
- **Effet :** ouvre brièvement un canal thermique et inflige **110 % → 260 %** de puissance technologique à la cible visée.
- Applique **1 → 3 Brûlures** selon le rang. Le rayon dure moins d'une demi-seconde et n'immobilise pas le Scientist.
- Sur une cible gelée, provoque un Choc thermique et brise immédiatement le gel.
- **Synergies :** +2 % de dégâts de Brûlure par point dans **Combustion catalytique**. Chaque point dans **Flashover** augmente de 1 % les dégâts du Choc thermique.

### 2. Combustion catalytique

- **Type :** passif — niveau 1.
- **Effet :** les dégâts de feu appliquent des Brûlures. Le maximum passe de **4 charges au rang 1 à 10 charges au rang 20**.
- Chaque charge inflige **15 % → 35 %** de puissance technologique sur 4 s. Une nouvelle charge rafraîchit seulement sa propre durée et ne remet pas automatiquement toutes les autres à leur maximum.
- Les dégâts de Brûlure sont attribués au Scientist qui les a appliqués pour l'expérience, le loot, les passifs et les quêtes.
- **Synergies :** augmente les dégâts d'**Inciseur thermique** et de **Flashover**. Les cibles brûlantes gagnent plus rapidement de la Conductivité grâce à **Singularité électrique**.

### 3. Nappe de prométhium

- **Type :** actif, zone au curseur — niveau 6 ; requiert Inciseur thermique.
- **Coût / recharge :** 20 Énergie ; 12 s.
- **Effet :** ouvre un réservoir de combustible expérimental sous la zone visée. Le rayon passe de **5 m à 9 m**, la durée de **6 s à 10 s** et les dégâts de **35 % à 90 %** de puissance technologique par seconde.
- Applique une Brûlure par seconde aux ennemis restant dans la nappe.
- Une Plaque de givre superposée crée une **Vapeur de confinement** : dégâts de feu et de glace réduits de 20 %, mais les ennemis présents infligent 10 % de dégâts en moins et gagnent deux fois plus vite Cryostase et Conductivité.
- **Synergies :** +2 % de dégâts par point dans **Inciseur thermique** ; +1 % de durée par point dans **Propagation pyrolytique**.

### 4. Propagation pyrolytique

- **Type :** passif — niveau 12 ; requiert Combustion catalytique.
- **Effet :** lorsqu'un ennemi brûlant meurt, **20 % → 100 %** des dégâts de Brûlure restant à infliger sont répartis entre **1 → 4 ennemis** proches dans un rayon de **4 m → 8 m**.
- Une même mort ne peut transmettre qu'une fois ses Brûlures, mais les nouvelles cibles peuvent à leur tour les propager lorsqu'elles meurent.
- Si aucune cible n'est disponible, les Brûlures perdues alimentent Cœur de four noir comme si un ennemi brûlant supplémentaire était présent pendant 2 s.
- **Synergies :** chaque point augmente de 1 % la durée de **Nappe de prométhium** et de 0,5 % le rayon de **Corona de fusion**.

### 5. Flashover

- **Type :** actif, zone au curseur — niveau 18 ; requiert Nappe de prométhium.
- **Coût / recharge :** 24 Énergie ; **10 s → 7 s**.
- **Effet :** provoque une combustion éclair dans un rayon de **5 m → 8 m**. Inflige **100 % → 240 %** de puissance technologique puis consume toutes les Brûlures des cibles.
- Ajoute immédiatement **50 % → 90 %** des dégâts persistants restant à infliger par les Brûlures consommées.
- Les cibles au maximum de Brûlure explosent dans un rayon de 3 m grâce à Entropie affamée.
- **Synergies :** +2 % de puissance de détonation par point dans **Combustion catalytique** et +1 % de rayon par tranche de 5 points dans **Propagation pyrolytique**. Sur une cible gelée, ajoute un Choc thermique distinct.

### 6. Cœur de four noir

- **Type :** passif — niveau 24 ; requiert Propagation pyrolytique.
- **Effet :** chaque ennemi brûlant dans un rayon de 12 m accorde **0,5 % → 2 %** de dégâts de feu et régénère **0,1 → 0,4 Énergie par seconde**.
- Le nombre d'ennemis pris en compte passe de **5 à 10** selon le rang.
- Au maximum de cibles comptées, le Scientist résiste à 50 % des dégâts provenant de ses propres zones de feu lorsque les dégâts personnels sont actifs.
- **Synergies :** chaque point augmente de 0,5 % les dégâts de **Corona de fusion**. Les ennemis artificiellement comptés par une propagation sans cible ne peuvent pas dépasser le plafond normal.

### 7. Corona de fusion

- **Type :** actif, zone autour de soi — niveau 30 ; requiert Flashover.
- **Coût / recharge :** 38 Énergie ; **20 s → 14 s**.
- **Effet :** ouvre autour du Scientist une couronne thermique de **6 m → 10 m**, infligeant **240 % → 600 %** de puissance technologique et appliquant immédiatement le nombre maximal de Brûlures permis par le rang de Combustion catalytique.
- La couronne reste 4 s autour du Scientist et inflige 25 % des dégâts initiaux par seconde aux ennemis proches.
- Le personnage peut courir, sauter et glisser pendant la durée.
- **Synergies :** +2 % de dégâts par point dans **Nappe de prométhium** et +0,5 % de rayon par point dans **Propagation pyrolytique**. Les ennemis gelés subissent Choc thermique avant de recevoir leurs nouvelles Brûlures.

### 8. Entropie affamée

- **Type :** passif — niveau 30 ; requiert Cœur de four noir.
- **Effet :** une cible atteignant son maximum de Brûlures devient **Critique**. Le prochain impact de feu déclenche une micro-explosion de **40 % → 150 %** de puissance technologique dans un rayon de 3 m et consomme deux Brûlures.
- Flashover sur une cible Critique ne consume que 70 % de ses Brûlures, arrondies au supérieur, ce qui permet de relancer plus vite la boucle.
- Les micro-explosions peuvent transmettre une Brûlure aux ennemis touchés mais ne se déclenchent pas récursivement entre elles.
- **Synergies :** +1 % de dégâts d'explosion par point dans **Flashover**. Une cible Surchargée électriquement augmente de 30 % le rayon de la micro-explosion.

## 4.2. Arbre Cryogénie de confinement

### Identité

Arbre de contrôle et de dégâts directs modérés. Il ralentit, gèle, fragilise puis brise les ennemis, tout en offrant au Scientist une défense ponctuelle.

```text
Aiguille cryonique ─► Plaque de givre industriel ─► Fracture cryogénique ─► Zéro local
Cryostase cumulative ─► Fragilité cristalline ─► Échangeur cryonique ─► Mort thermique
```

### 1. Aiguille cryonique

- **Type :** actif, projectile monocible — niveau 1.
- **Coût / recharge :** 10 Énergie ; **4 s → 2,8 s**.
- **Effet :** projette une aiguille condensée infligeant **120 % → 300 %** de puissance technologique et une forte quantité de Cryostase.
- Aux rangs 10 et 20, l'aiguille traverse respectivement un puis deux ennemis supplémentaires, sans correction de trajectoire.
- Sur une cible brûlante, elle réduit de moitié une Brûlure et crée une petite Vapeur de confinement pendant 2 s.
- **Synergies :** +2 % de dégâts par point dans **Cryostase cumulative**. Chaque point dans **Fragilité cristalline** augmente de 1 % ses dégâts contre une cible déjà ralentie.

### 2. Cryostase cumulative

- **Type :** passif — niveau 1.
- **Effet :** les dégâts de glace appliquent **10 → 25 unités de Cryostase**. À 100, un ennemi normal est gelé pendant **1,5 s → 4 s**.
- Avant le gel, la Cryostase ralentit progressivement jusqu'à un maximum de 50 %.
- Les boss ne sont pas immobilisés ; à 100, ils deviennent **Cryofragiles** pendant la durée prévue, subissent le ralentissement maximal autorisé et peuvent être brisés par les compétences correspondantes.
- **Synergies :** augmente les dégâts d'**Aiguille cryonique**. Chaque point augmente de 0,5 % la génération de Cryostase de **Plaque de givre industriel**.

### 3. Plaque de givre industriel

- **Type :** actif, zone au curseur — niveau 6 ; requiert Aiguille cryonique.
- **Coût / recharge :** 18 Énergie ; 12 s.
- **Effet :** couvre le sol dans un rayon de **5 m → 9 m** pendant **6 s → 10 s**. Inflige **30 % → 75 %** de puissance technologique par seconde et ralentit de **20 % → 50 %**.
- Applique continuellement de la Cryostase. Les ennemis entrant à grande vitesse peuvent glisser brièvement selon leur masse et leur animation.
- Superposée à Nappe de prométhium, crée Vapeur de confinement au lieu d'annuler simplement les deux zones.
- **Synergies :** +1,5 % de dégâts par point dans **Aiguille cryonique** et +0,5 % de Cryostase par point dans **Cryostase cumulative**.

### 4. Fragilité cristalline

- **Type :** passif — niveau 12 ; requiert Cryostase cumulative.
- **Effet :** les ennemis ralentis par Cryostase subissent **3 % → 18 %** de dégâts physiques et explosifs supplémentaires.
- La mort d'un ennemi gelé le fait éclater et inflige **60 % → 220 %** de puissance technologique dans un rayon de **3 m → 6 m**.
- Les machines se fragmentent en pièces refroidies ; les organismes se brisent dans un effet gore gelé adapté à leur anatomie.
- **Synergies :** chaque point augmente de 3 % les dégâts de **Fracture cryogénique** contre une cible gelée et de 1 % les dégâts de Shrapnel industriel contre les cibles ralenties.

### 5. Fracture cryogénique

- **Type :** actif, zone au curseur — niveau 18 ; requiert Plaque de givre industriel.
- **Coût / recharge :** 24 Énergie ; **11 s → 8 s**.
- **Effet :** brise violemment la température d'une zone de **5 m → 8 m** et inflige **180 % → 440 %** de puissance technologique.
- Les ennemis gelés ou Cryofragiles subissent +100 % de dégâts, déclenchent leur éclatement et perdent leur état de gel.
- Les ennemis simplement ralentis reçoivent 25 unités supplémentaires de Cryostase.
- **Synergies :** +3 % de dégâts contre les cibles gelées par point dans **Fragilité cristalline**. Flashover lancé immédiatement après produit un Choc thermique sur toutes les cibles brisées.

### 6. Échangeur cryonique

- **Type :** passif — niveau 24 ; requiert Fragilité cristalline.
- **Effet :** geler un ennemi restaure **1 → 6 Énergie** et **1 % → 4 %** du bouclier maximal, avec une recharge interne de 0,5 s par cible.
- Tuer un ennemi gelé réduit les recharges des compétences de glace de **0,2 s → 1 s**.
- Sur un boss Cryofragile, chaque déclenchement complet à 100 Cryostase active la restauration une fois, même sans immobilisation.
- **Synergies :** chaque point augmente de 1 % la durée de l'armure produite par **Zéro local** et de 0,5 % la restitution de Ralliement traumatique reçue par le Scientist.

### 7. Zéro local

- **Type :** actif, zone autour de soi — niveau 30 ; requiert Fracture cryogénique.
- **Coût / recharge :** 36 Énergie ; **24 s → 18 s**.
- **Effet :** aspire brutalement la chaleur dans un rayon de 8 m, inflige **200 % → 500 %** de puissance technologique et ajoute **60 → 100 Cryostase**.
- Le Scientist reçoit pendant **3 s → 6 s** une armure condensée réduisant les dégâts subis de **10 % → 25 %**.
- Les projectiles ennemis déjà présents dans la zone sont ralentis visuellement et mécaniquement de 20 %, sans être supprimés.
- **Synergies :** +2 % de dégâts par point dans **Plaque de givre industriel** et +1 % de durée d'armure par point dans **Échangeur cryonique**. Une Corona de fusion lancée pendant l'armure provoque Choc thermique autour du Scientist sans détruire immédiatement sa protection.

### 8. Mort thermique

- **Type :** passif — niveau 30 ; requiert Échangeur cryonique.
- **Effet :** après la fin d'un gel ou d'un état Cryofragile, la cible reste **Cassante** pendant **4 s → 10 s** et subit **10 % → 30 %** de dégâts supplémentaires de toutes les sources.
- Le feu appliqué à une cible gelée ou Cassante provoque **Choc thermique** : consume le gel restant et inflige **80 % → 260 %** de puissance technologique dans une petite zone.
- La foudre sur une cible gelée ou Cassante gagne les effets de Superconduction.
- **Synergies :** +1 % de dégâts de Choc thermique par point dans **Fracture cryogénique**. Ce passif constitue le principal pont entre les trois écoles du Scientist et les dégâts physiques du groupe.

## 4.3. Arbre Électrocinétique de rupture

### Identité

Arbre de dégâts intermédiaires, de propagation automatique et de contrôle léger. Il accumule de la Conductivité, multiplie les rebonds et transforme les cibles chargées en détonateurs électriques.

```text
Arc voltaïque ─► Lance capacitive ─► Orbe de Faraday ─► Tempête carcérale
Charge conductrice ─► Superconduction ─► Réseau de surtension ─► Singularité électrique
```

### 1. Arc voltaïque

- **Type :** actif, chaîne de cibles — niveau 1.
- **Coût / recharge :** 11 Énergie ; **4,5 s → 3,2 s**.
- **Effet :** frappe la cible visée pour **100 % → 240 %** de puissance technologique puis rebondit vers **2 → 6 cibles** supplémentaires.
- Chaque rebond perd **20 % → 5 %** de dégâts et cherche une cible dans un rayon de **5 m → 9 m**.
- L'arc ne corrige pas le premier tir : le joueur doit réellement viser la cible initiale.
- **Synergies :** +2 % de dégâts par point dans **Charge conductrice**. Les cibles ralenties ou Cassantes permettent les rebonds supplémentaires de Superconduction.

### 2. Charge conductrice

- **Type :** passif — niveau 1.
- **Effet :** les dégâts électriques appliquent Conductivité. Le maximum passe de **3 charges au rang 1 à 8 charges au rang 20**.
- Chaque charge augmente les dégâts électriques reçus de **1 % → 3 %** et la portée de rebond de 0,2 m.
- Les charges durent **4 s → 10 s**. Une nouvelle charge rafraîchit seulement la plus ancienne.
- **Synergies :** augmente les dégâts d'**Arc voltaïque** et la puissance consommée par **Lance capacitive**.

### 3. Lance capacitive

- **Type :** actif, monocible — niveau 6 ; requiert Arc voltaïque.
- **Coût / recharge :** 18 Énergie ; **8 s → 6 s**.
- **Effet :** concentre la charge dans un trait électrique infligeant **180 % → 480 %** de puissance technologique.
- Consomme toutes les Conductivités de la cible ; chaque charge ajoute **15 % → 30 %** de dégâts et 0,15 s de stagger, plafonné à 1,2 s sur les ennemis normaux.
- Sur une cible brûlante, une partie des Brûlures peut être ionisée grâce à Singularité électrique.
- **Synergies :** +2 % de dégâts de base par point dans **Arc voltaïque**. Chaque point dans **Charge conductrice** augmente de 1 % le bonus par charge consommée.

### 4. Superconduction

- **Type :** passif — niveau 12 ; requiert Charge conductrice.
- **Effet :** contre une cible ralentie, gelée ou Cassante, les chaînes électriques gagnent **10 % → 50 %** de portée et un rebond supplémentaire aux rangs 7, 14 et 20.
- Chaque rebond transfère **10 % → 50 %** de la Cryostase de la cible précédente vers la suivante.
- Une cible gelée frappée par Lance capacitive n'est pas immédiatement libérée ; la durée restante est réduite de moitié au lieu d'être supprimée.
- **Synergies :** chaque point augmente de 1,5 % les dégâts d'**Orbe de Faraday** contre les cibles sous Cryostase. **Mort thermique** prolonge les conditions de Superconduction après le dégel.

### 5. Orbe de Faraday

- **Type :** actif, orbe mobile placé au curseur — niveau 18 ; requiert Lance capacitive.
- **Coût / recharge :** 26 Énergie ; **14 s → 10 s**.
- **Effet :** lance une sphère de confinement lente pendant **6 s → 10 s**. Elle frappe **2 → 6 ennemis par seconde** dans un rayon de **5 m → 8 m**, chaque arc infligeant **35 % → 85 %** de puissance technologique.
- L'orbe avance vers le point désigné puis demeure sur place. Elle peut passer au-dessus de petits obstacles mais respecte les murs et volumes majeurs.
- Chaque cible ne peut être frappée qu'une fois par intervalle de 0,4 s.
- **Synergies :** +1,5 % de dégâts par point dans **Superconduction** contre les cibles froides. Une Nappe de prométhium traversée par l'Orbe ionise ses ennemis brûlants et leur ajoute une Conductivité toutes les 2 s.

### 6. Réseau de surtension

- **Type :** passif — niveau 24 ; requiert Superconduction.
- **Effet :** la mort d'un ennemi conducteur libère **1 → 5 arcs** vers des cibles proches, chacun infligeant **30 % → 100 %** de puissance technologique, et restaure **1 → 5 Énergie**.
- Sur un boss à Conductivité maximale, chaque tranche de 5 % de vie perdue libère une salve réduite, avec une recharge interne de 2 s.
- Les arcs de mort transmettent la moitié des Conductivités restantes mais ne peuvent pas déclencher immédiatement une nouvelle salve sur la même cible.
- **Synergies :** chaque point augmente de 1 % la fréquence de frappe de **Tempête carcérale** et de 0,5 % la durée de **Orbe de Faraday**.

### 7. Tempête carcérale

- **Type :** actif, grande zone au curseur — niveau 30 ; requiert Orbe de Faraday.
- **Coût / recharge :** 40 Énergie ; **24 s → 18 s**.
- **Effet :** enferme une zone de **7 m → 11 m** pendant **5 s → 9 s** dans une cage d'arcs. Elle frappe **3 → 8 cibles par seconde** pour **70 % → 150 %** de puissance technologique.
- Une cible à Conductivité maximale peut être frappée deux fois pendant une même seconde.
- Les machines subissent 25 % de stagger supplémentaire ; les organismes présentent des convulsions et réactions gore propres à l'électricité.
- **Synergies :** +2 % de dégâts par point dans **Orbe de Faraday** et +1 % de fréquence par point dans **Réseau de surtension**. Une Plaque de givre située dans la zone active automatiquement Superconduction.

### 8. Singularité électrique

- **Type :** passif — niveau 30 ; requiert Réseau de surtension.
- **Effet :** une cible atteignant son maximum de Conductivité devient **Surchargée**. La prochaine compétence électrique consommant ou frappant fortement ses charges déclenche une explosion de **80 % → 260 %** de puissance technologique dans un rayon de **3 m → 6 m** et transmet la moitié des charges aux ennemis touchés.
- Si la cible brûle, l'explosion ionise **25 % → 60 %** des dégâts de Brûlure restant à infliger et les ajoute instantanément à la détonation, sans consommer toutes les Brûlures.
- Une cible gelée augmente le rayon de 30 % grâce à Superconduction.
- **Synergies :** +1 % de dégâts de surcharge par point dans **Lance capacitive** et +0,5 % de rayon par point dans **Réseau de surtension**.

## 4.4. Réactions élémentaires structurantes

1. **Feu + Feu — Flashover** : accumule des Brûlures puis convertit une partie du temps restant en dégâts instantanés.
2. **Glace + Glace — Fracture cryogénique** : construit Cryostase, gèle puis brise les cibles.
3. **Foudre + Foudre — Surcharge** : accumule Conductivité, la consume avec Lance capacitive ou la fait exploser par Singularité électrique.
4. **Feu + Glace — Choc thermique** : brise le gel, inflige une explosion technologique et ouvre une fenêtre Cassante.
5. **Glace + Foudre — Superconduction** : augmente portée et nombre de rebonds, puis transfère la Cryostase à travers les chaînes.
6. **Feu + Foudre — Ionisation** : les Brûlures accélèrent la Conductivité ; une Surcharge convertit une partie du feu restant en explosion immédiate.
7. **Nappe de prométhium + Plaque de givre — Vapeur de confinement** : zone moins létale mais très forte pour le contrôle, la réduction de dégâts ennemis et la préparation électrique.
8. **Mort thermique** : laisse les ennemis Cassants après le dégel et transforme le contrôle de glace en fenêtre de dégâts pour tout le groupe.

---
# 5. Engineer

L'Engineer est un nécromancien industriel sans morts-vivants : il récupère des châssis, réécrit des protocoles, lance des micro-usines et condamne des zones entières par corrosion, brouillage et anomalies gravifiques.

Toutes ses unités sont mécaniques :

- un seul **compagnon principal permanent** peut être actif ;
- plusieurs **robots et drones temporaires** peuvent l'accompagner ;
- les champs de perturbation rendent les ennemis plus vulnérables aux machines et aux alliés.

Les dégâts, éliminations et objectifs réalisés par les robots sont attribués à leur propriétaire pour l'expérience, le loot, les passifs et les quêtes.

## 5.1. Arbre Châssis principal

### Identité

Cet arbre propose trois modèles exclusifs de compagnon principal. Invoquer un modèle remplace le précédent ; un seul existe à la fois. Une nouvelle invocation conserve le pourcentage de vie du compagnon remplacé et partage une recharge de déploiement, empêchant de changer gratuitement de châssis pour le soigner.

Une première activation déploie le robot. Une nouvelle activation lorsque ce modèle est déjà présent lui ordonne d'utiliser sa capacité signature au point ou sur la cible visée.

```text
Molosse K-9 « Charognard » ─► Bastion H-0 « Porte-Cercueil » ─► Suture-3 « Chirurgien » ─► Surcharge interdite
Blindage cannibale ─► Protocoles de prédation ─► Noyau interchangeable ─► Reconstruction impie
```

### 1. Molosse K-9 « Charognard »

- **Type :** actif, compagnon principal permanent — niveau 1.
- **Coût / recharge :** 24 Énergie pour le déploiement ; 8 s de recharge partagée entre châssis.
- **Rôle :** robot terrestre d'assaut rapide, équipé de mâchoires hydrauliques et d'une arme dorsale courte portée.
- **Progression :** ses dégâts passent de **100 % à 240 %** de la valeur de base du compagnon et sa vitesse de déplacement de **100 % à 150 %**.
- **Commande signature :** bondit sur la cible ou le point visé et inflige **180 % → 450 %** de dégâts de compagnon dans un rayon de **2,5 m → 5 m**. Recharge de commande : **8 s → 5 s**.
- **Synergies :** +2 % de vie par point dans **Blindage cannibale** et +1,5 % de dégâts par point dans **Protocoles de prédation**. Le bond attire la priorité des Scarabées de découpe vers la cible touchée.

### 2. Blindage cannibale

- **Type :** passif — niveau 1.
- **Effet :** tous les compagnons principaux gagnent **1,5 % de vie maximale** et **1 % d'efficacité d'armure par rang**, soit +30 % de vie et +20 % d'armure au rang 20.
- La destruction d'une machine ennemie à moins de 12 m répare le compagnon de **0,5 % → 4 %** de sa vie, avec une recharge interne de 1 s.
- Les ennemis biologiques équipés d'implants ou d'armures mécaniques peuvent laisser une quantité réduite de pièces récupérables si leur profil le permet.
- **Synergies :** augmente la robustesse des trois châssis et la quantité de vie rendue par **Reconstruction impie**.

### 3. Bastion H-0 « Porte-Cercueil »

- **Type :** actif, compagnon principal permanent — niveau 6 ; requiert Molosse K-9.
- **Coût / recharge :** 28 Énergie ; recharge partagée de 8 s.
- **Rôle :** automate lourd sur chenilles ou pattes renforcées, doté d'un bouclier frontal et d'un générateur de menace.
- **Progression :** possède **180 % → 360 %** de la vie de base du compagnon, mais seulement 70 % des dégâts du Molosse.
- **Commande signature :** déploie pendant **4 s → 8 s** un écran frontal, réduit ses dégâts reçus de **30 % → 60 %** et provoque les ennemis normaux dans un rayon de **5 m → 9 m**. Les boss augmentent leur priorité de menace sans être forcés.
- **Synergies :** +2 % de réduction de recharge de commande par point dans **Blindage cannibale**. Dans un **Champ corrosif**, son bouclier renvoie une petite impulsion de corrosion à chaque blocage, au maximum une fois par seconde.

### 4. Protocoles de prédation

- **Type :** passif — niveau 12 ; requiert Blindage cannibale.
- **Effet :** augmente les dégâts du compagnon principal de **1,5 % par rang**, soit +30 % au rang 20.
- Chaque type de débuff présent sur la cible ajoute **0,5 % → 2 %** de dégâts supplémentaires au compagnon, jusqu'à cinq types.
- Le compagnon reçoit une priorité comportementale claire : cible marquée par Directive Zéro, cible récemment frappée par le propriétaire, puis ennemi menaçant le propriétaire.
- Ce ciblage ne donne aucun bonus de précision ; il ne fait que déterminer la cible attaquée par l'IA.
- **Synergies :** renforce les trois châssis et constitue le premier lien direct avec **Protocoles de condamnation**.

### 5. Suture-3 « Chirurgien »

- **Type :** actif, compagnon principal permanent — niveau 18 ; requiert Bastion H-0.
- **Coût / recharge :** 30 Énergie ; recharge partagée de 8 s.
- **Rôle :** drone de soutien volant équipé de pinces, injecteurs, plaques de remplacement et projecteur de bouclier. Il attaque faiblement entre ses opérations de soutien.
- **Progression :** sa cadence de réparation et la résistance de son châssis augmentent de **100 % à 220 %** de leurs valeurs de base.
- **Commande signature :** restaure **8 % → 25 %** du bouclier maximal des alliés proches et du compagnon, ou **4 % → 12 %** de vie aux cibles sans bouclier actif. Recharge : **12 s → 8 s**.
- **Synergies :** +1 % de restauration par point dans **Blindage cannibale**. Les **Essaims de maintenance** prolongent de 0,5 s par drone leur présence lorsqu'ils réparent Suture-3.

### 6. Noyau interchangeable

- **Type :** passif — niveau 24 ; requiert Protocoles de prédation.
- **Effet :** le compagnon principal hérite de **5 % → 40 %** du bonus élémentaire le plus élevé du propriétaire.
- Ses attaques appliquent à **10 % → 50 %** de leur puissance normale l'état correspondant : Brûlure, Cryostase ou Conductivité.
- Un objet spécifique peut verrouiller le noyau sur un élément même si ce n'est pas le bonus le plus élevé. Sans spécialisation élémentaire, le noyau reste cinétique et applique une faible Fracture mécanique.
- **Synergies :** permet aux robots de participer directement aux réactions du Scientist. Chaque point augmente de 1 % les dégâts élémentaires de **Surcharge interdite**.

### 7. Surcharge interdite

- **Type :** actif, buff du compagnon principal — niveau 30 ; requiert Suture-3.
- **Coût / recharge :** 36 Énergie ; **25 s → 18 s**.
- **Effet :** force le réacteur du compagnon pendant **6 s → 12 s**. Il gagne **20 % → 50 %** de vitesse d'attaque, **15 % → 40 %** de vitesse de déplacement et voit la recharge de sa commande signature divisée par deux.
- Le châssis perd 2 % de sa vie maximale par seconde pendant la surcharge ; cette perte ne peut pas le tuer et s'arrête à 1 PV.
- Molosse gagne une seconde onde à l'atterrissage, Bastion élargit son écran, Suture-3 pulse une petite réparation toutes les 2 s.
- **Synergies :** +1 % de durée par point dans **Reconstruction impie** et +1 % de dégâts élémentaires par point dans **Noyau interchangeable**.

### 8. Reconstruction impie

- **Type :** passif — niveau 30 ; requiert Noyau interchangeable.
- **Effet :** lorsqu'il est détruit, le compagnon principal se reconstruit automatiquement après **25 s → 8 s** avec **20 % → 80 %** de sa vie maximale.
- Chaque élimination attribuée au propriétaire pendant la reconstruction réduit le délai de **0,2 s → 1 s** ; les élites comptent quadruple.
- Remplacer volontairement un châssis conserve le pourcentage de vie, les statuts négatifs et les recharges de commande. Cette règle empêche le changement de modèle de servir de soin gratuit.
- À partir du rang 15, la destruction du compagnon libère trois microdrones temporaires défensifs pendant 6 s.
- **Synergies :** chaque point dans **Blindage cannibale** augmente de 1 % la vie de retour. **Réplication de terrain** transforme les débris du compagnon détruit en plusieurs unités de ferraille.

## 5.2. Arbre Essaim industriel

### Identité

Arbre des petites unités temporaires. Les drones attaquent, découpent ou réparent pendant une durée limitée. La **Bande passante** fixe combien d'unités peuvent être actives, tandis que les débris et batteries prolongent ou renouvellent la nuée. La limite globale de base est de 4 unités au niveau 1, puis gagne 1 emplacement aux niveaux 20, 40, 60 et 80 ; **Bande passante volée** peut encore ajouter jusqu'à 5 emplacements. Un Engineer de niveau 99 peut donc atteindre 13 unités temporaires avant d'éventuels effets uniques d'équipement.

```text
Vautours R-4 ─► Scarabées de découpe ─► Essaim de maintenance ─► Protocole Kamikaze
Bande passante volée ─► Batteries de charnier ─► Réplication de terrain ─► Nuée autonome
```

### 1. Vautours R-4

- **Type :** actif, invocation temporaire au curseur — niveau 1.
- **Coût / recharge :** 20 Énergie ; **12 s → 8 s**.
- **Effet :** déploie **2 → 5 drones volants** pendant **8 s → 16 s**. Ils tirent sur les ennemis proches de la zone désignée puis suivent l'Engineer.
- Chaque drone inflige **35 % → 85 %** de puissance de compagnon par salve.
- Une nouvelle activation déplace leur balise de priorité vers le point visé sans prolonger leur durée.
- **Synergies :** le nombre et la durée profitent de **Bande passante volée**. Les cibles sous Directive Zéro sont toujours prioritaires.

### 2. Bande passante volée

- **Type :** passif — niveau 1.
- **Effet :** augmente la durée de tous les drones temporaires de **1,5 % par rang**, soit +30 % au rang 20.
- La limite globale d'unités temporaires augmente de 1 aux rangs 4, 8, 12, 16 et 20, soit +5 unités.
- Le rayon dans lequel les drones acceptent les ordres et suivent l'Engineer passe de **15 m à 30 m**.
- Si une invocation dépasserait la limite, les unités les plus anciennes expirent proprement et peuvent produire des débris.
- **Synergies :** augmente directement Vautours, Scarabées, Maintenance et le nombre de détonations de **Protocole Kamikaze**.

### 3. Scarabées de découpe

- **Type :** actif, invocation temporaire sur une cible ou une zone — niveau 6 ; requiert Vautours R-4.
- **Coût / recharge :** 24 Énergie ; **14 s → 9 s**.
- **Effet :** libère **3 → 8 microbots terrestres** pendant **6 s → 12 s**. Ils se ruent sur les ennemis, grimpent sur les blindages et infligent chacun **30 % → 70 %** de puissance de compagnon par seconde.
- Trois Scarabées attaquant la même cible appliquent une charge de Corrosion toutes les 2 s.
- Ils franchissent les petits obstacles mais ne téléportent pas à travers les murs.
- **Synergies :** +1,5 % de dégâts par point dans **Batteries de charnier**. Le bond du Molosse et Puits gravimétrique concentrent naturellement leurs cibles.

### 4. Batteries de charnier

- **Type :** passif — niveau 12 ; requiert Bande passante volée.
- **Effet :** une élimination à moins de 8 m d'un drone prolonge sa durée de **0,2 s → 1 s**, jusqu'à 50 % de sa durée initiale supplémentaire.
- Lorsqu'un drone expire ou est détruit, il laisse une batterie pendant 4 s. Le premier autre drone à la toucher récupère **5 % → 30 %** de vie et **0,5 s → 3 s** de durée.
- Les batteries inutilisées deviennent de la ferraille pour Réplication de terrain.
- **Synergies :** chaque point augmente de 1 % les dégâts des **Scarabées de découpe** et de 0,5 % la durée des **Essaims de maintenance**.

### 5. Essaim de maintenance

- **Type :** actif, invocation temporaire autour d'un allié ou de soi — niveau 18 ; requiert Scarabées de découpe.
- **Coût / recharge :** 28 Énergie ; **16 s → 11 s**.
- **Effet :** déploie **2 → 6 drones réparateurs** pendant **8 s → 14 s**. Ils orbitent autour de la cible choisie et restaurent ensemble **1 % → 4 %** de bouclier maximal par seconde.
- Sur le compagnon principal, ils restaurent à la place **1,5 % → 6 %** de vie par seconde. Sur un joueur sans bouclier actif, la réparation de vie est réduite à 40 % de sa valeur.
- Ils peuvent changer de cible si celle-ci est entièrement restaurée et qu'un allié proche est gravement endommagé.
- **Synergies :** +1 % de réparation par point dans **Réplication de terrain**. Suture-3 augmente leur durée lorsqu'il reçoit leurs réparations.

### 6. Réplication de terrain

- **Type :** passif — niveau 24 ; requiert Batteries de charnier.
- **Effet :** les drones détruits, expirés ou les batteries inutilisées produisent de la ferraille. Toutes les **8 → 3 unités de ferraille**, une micro-usine crée gratuitement un Vautour ou un Scarabée du dernier type offensif invoqué.
- L'unité répliquée possède 50 % de la durée normale et ne peut elle-même produire qu'une demi-unité de ferraille, empêchant une réplication infinie.
- Les éliminations du compagnon principal produisent une ferraille, avec une recharge interne de **4 s → 1,5 s**.
- **Synergies :** augmente les réparations de **Essaim de maintenance**. La destruction du compagnon principal sous Reconstruction impie produit immédiatement **2 → 8 ferrailles**.

### 7. Protocole Kamikaze

- **Type :** actif, ordre de zone au curseur — niveau 30 ; requiert Essaim de maintenance.
- **Coût / recharge :** 30 Énergie ; **12 s → 7 s**.
- **Effet :** ordonne à tous les drones temporaires offensifs de plonger sur le point visé et d'exploser. Chaque drone inflige **100 % → 300 %** de puissance de compagnon dans un rayon de 3 m.
- Les Vautours produisent une explosion électrique, les Scarabées une explosion physique et corrosive. Les drones de maintenance ne se détruisent pas : ils convergent et créent à la place un bouclier de **2 % → 8 %** par drone autour du point.
- Les drones consommés produisent normalement de la ferraille.
- **Synergies :** +1 % de dégâts par point investi dans **Vautours R-4** et **Scarabées de découpe**. Les cibles Exposées subissent +30 % de dégâts de Kamikaze.

### 8. Nuée autonome

- **Type :** passif — niveau 30 ; requiert Réplication de terrain.
- **Effet :** chaque drone actif gagne **0,5 % → 2 %** de dégâts ou de réparation par autre drone temporaire, avec un plafond de 20 %.
- Les drones répartissent intelligemment leurs cibles : ils évitent de surcharger une cible normale déjà condamnée, sauf si elle porte Directive Zéro.
- Chaque **5e → 2e drone** détruit ou expiré crée automatiquement un mini-drone de 4 s ne comptant pas dans la limite de Bande passante. Le mini-drone ne produit ni ferraille ni nouveau mini-drone.
- Trois types de drones différents présents simultanément accordent à tous **5 % → 20 %** de vitesse d'action.
- **Synergies :** maximise les invocations mixtes et renforce **Protocole Kamikaze** sans exiger un seul type de drone.

## 5.3. Arbre Guerre électronique et condamnation

### Identité

Équivalent futuriste des malédictions. Cet arbre corrode les protections, brouille les actions ennemies, regroupe les cibles et désigne des zones d'exécution pour les robots comme pour les joueurs.

```text
Champ corrosif ─► Impulsion EMP ─► Puits gravimétrique ─► Directive Zéro
Algorithme d'exploitation ─► Propagation virale ─► Protocoles de condamnation ─► Réseau de supplice
```

### 1. Champ corrosif

- **Type :** actif, zone au curseur — niveau 1.
- **Coût / recharge :** 16 Énergie ; **12 s → 8 s**.
- **Effet :** déploie pendant **6 s → 10 s** un brouillard de nanites dans un rayon de **5 m → 9 m**.
- Chaque seconde applique une Corrosion, jusqu'à 5. Chaque charge réduit armure et résistances de **1 % → 3 %**, soit jusqu'à 15 % au rang 20.
- Inflige de faibles dégâts technologiques continus et possède des effets visuels différents sur chair, métal et matière extradimensionnelle.
- **Synergies :** +1 % de rayon et de durée cumulés par point dans **Algorithme d'exploitation**. Les Scarabées appliquent plus rapidement la Corrosion aux cibles présentes.

### 2. Algorithme d'exploitation

- **Type :** passif — niveau 1.
- **Effet :** le compagnon principal et les drones infligent **1 % → 3 %** de dégâts supplémentaires par type de débuff présent sur la cible, jusqu'à **10 % → 30 %**.
- Les joueurs alliés reçoivent 40 % de ce bonus contre les cibles affectées par au moins un débuff de l'Engineer.
- Les charges d'un même débuff ne comptent pas comme plusieurs types, ce qui favorise la combinaison Corrosion, Brouillage, ralentissement gravifique et Exposition.
- **Synergies :** augmente le rayon et la durée de **Champ corrosif** et les dégâts accumulés par **Directive Zéro**.

### 3. Impulsion EMP

- **Type :** actif, zone autour de soi — niveau 6 ; requiert Champ corrosif.
- **Coût / recharge :** 20 Énergie ; **14 s → 9 s**.
- **Effet :** libère une impulsion de **6 m → 12 m** infligeant **100 % → 250 %** de puissance technologique et appliquant **Brouillage** pendant **3 s → 8 s**.
- Brouillage réduit la vitesse d'attaque et de préparation des capacités ennemies de **10 % → 30 %**. Les machines sont en plus désactivées pendant **1 s → 3 s** ; les boss mécaniques subissent un fort stagger au lieu d'une désactivation complète.
- N'affecte jamais la précision des ennemis ou des joueurs.
- **Synergies :** +2 % de dégâts par point dans **Propagation virale**. Les cibles Corrodées reçoivent une Conductivité par impulsion si Noyau interchangeable est électrique.

### 4. Propagation virale

- **Type :** passif — niveau 12 ; requiert Algorithme d'exploitation.
- **Effet :** lorsqu'un ennemi affecté par un débuff de l'Engineer meurt, il transmet **1 → 4 types de débuffs** à **1 → 5 ennemis** dans un rayon de **4 m → 9 m**.
- Les effets transmis conservent **30 % → 80 %** de leur durée restante et la moitié de leurs charges.
- Directive Zéro et les états propres aux boss ne sont jamais propagés.
- **Synergies :** augmente les dégâts d'**Impulsion EMP** et la durée de **Puits gravimétrique**. Une explosion Kamikaze sur une cible contaminée élargit la propagation de 20 %.

### 5. Puits gravimétrique

- **Type :** actif, zone au curseur — niveau 18 ; requiert Impulsion EMP.
- **Coût / recharge :** 28 Énergie ; **16 s → 11 s**.
- **Effet :** crée pendant **3 s → 7 s** une anomalie de **5 m → 8 m** qui attire les ennemis vers son centre et ralentit leur déplacement de **25 % → 60 %**.
- Les ennemis légers sont déplacés continuellement ; les lourds sont freinés ; les boss ne bougent pas mais subissent le ralentissement maximal qu'autorise leur profil.
- Les drones offensifs priorisent automatiquement les ennemis dans le Puits, sans recevoir de bonus de précision.
- **Synergies :** +1 % de durée par point dans **Propagation virale**. Treuil gravifique, Nappe de prométhium, Plaque de givre, Tempête carcérale et les explosifs profitent naturellement du regroupement.

### 6. Protocoles de condamnation

- **Type :** passif — niveau 24 ; requiert Propagation virale.
- **Effet :** une cible portant au moins deux types de débuffs de l'Engineer devient **Exposée**.
- Une cible Exposée subit **5 % → 20 %** de dégâts supplémentaires des compagnons et drones, ainsi que **2 % → 10 %** de dégâts supplémentaires des joueurs alliés.
- Avec trois types de débuffs, ses soins, réparations et régénérations sont réduits de 50 % ; avec quatre, elle produit une unité de ferraille supplémentaire à sa mort.
- **Synergies :** chaque point augmente de 0,5 % les dégâts du compagnon via **Protocoles de prédation**. Protocole Kamikaze reçoit son bonus majeur contre les cibles Exposées.

### 7. Directive Zéro

- **Type :** actif, cible prioritaire et petite zone — niveau 30 ; requiert Puits gravimétrique.
- **Coût / recharge :** 40 Énergie ; **24 s → 18 s**.
- **Effet :** condamne une cible majeure ou jusqu'à **6 → 12 ennemis normaux** dans un rayon de 6 m pendant **6 s → 12 s**.
- Tous les robots et drones alliés pouvant atteindre ces cibles les priorisent. Chaque type d'unité différent qui inflige des dégâts stocke une charge d'exécution.
- À la fin de la Directive ou à la mort de la cible, les charges explosent pour **200 % → 600 %** de puissance technologique, plus 50 % par type d'unité, avec un plafond de cinq types.
- Le ciblage imposé ne garantit pas les impacts et n'augmente pas la précision des unités.
- **Synergies :** +2 % de dégâts par point dans **Algorithme d'exploitation**. Puits gravimétrique maintient les cibles dans la zone ; Surcharge interdite accélère fortement l'accumulation des charges.

### 8. Réseau de supplice

- **Type :** passif — niveau 30 ; requiert Protocoles de condamnation.
- **Effet :** chaque impact d'un compagnon ou drone prolonge un débuff de l'Engineer de **0,05 s → 0,2 s**, avec un maximum de 5 s ajoutées par effet.
- Tous les **10e → 4e impacts** de machines alliées sur une cible Exposée déclenchent une mini-impulsion alternant Corrosion et Brouillage.
- Chaque robot ou drone actif réduit de **0,1 % → 0,5 %** les recharges des compétences de débuff, avec un plafond de 15 %.
- Les mini-impulsions ne peuvent pas se propager entre elles et possèdent une recharge interne par cible.
- **Synergies :** boucle finale entre le nombre de machines, la durée des affaiblissements et la fréquence des zones de contrôle.

## 5.4. Synergies structurantes de l'Engineer

1. **Choix de châssis** : Molosse pour l'assaut, Bastion pour le tanking, Suture-3 pour le soutien ; un seul existe à la fois.
2. **Blindage cannibale + Reconstruction impie** : rend le compagnon durable sans permettre de le soigner gratuitement par changement de modèle.
3. **Bande passante + Batteries + Réplication** : augmente le nombre, prolonge la durée puis transforme les débris en nouvelles unités.
4. **Puits gravimétrique → Scarabées / Kamikaze** : concentre les cibles pour les essaims terrestres et les détonations.
5. **Corrosion + Brouillage → Exposition** : les débuffs ne sont pas seulement défensifs ; ils ouvrent une fenêtre de dégâts pour tout le groupe.
6. **Directive Zéro + Surcharge interdite** : le compagnon et la nuée concentrent rapidement leurs impacts sur une cible prioritaire.
7. **Noyau interchangeable** : permet au compagnon robotique de participer aux Brûlures, à la Cryostase et à la Conductivité du Scientist.
8. **Protocole Kamikaze + Réplication de terrain** : sacrifie volontairement la nuée, puis recycle ses débris au lieu de perdre toute la boucle d'invocation.

---

# 6. Synergies entre classes en coopération

Ces interactions ne sont pas obligatoires pour jouer en solo, mais donnent aux groupes jusqu'à cinq joueurs des combinaisons immédiatement compréhensibles.

## Enforcer + Marine

- Treuil gravifique et Puits gravimétrique regroupent les ennemis pour Grenade M-13, Couronne de charges et Impact sismique.
- Marquage d'extermination amplifie Surcharge de canon et Double détente sur les élites.
- Ordre : Avancez améliore le repositionnement du Marine sans toucher à sa précision.
- Ordre : Tenez la ligne protège le Marine pendant les courtes préparations d'armes lourdes.

## Enforcer + Scientist

- Dôme de ferraille donne au Scientist l'espace nécessaire pour poser ses zones.
- Treuil gravifique rassemble les ennemis dans Nappe de prométhium, Plaque de givre ou Tempête carcérale.
- Présence de fer réduit les interruptions pendant l'utilisation des dispositifs scientifiques.
- Les cibles Cassantes de Mort thermique renforcent les dégâts physiques de l'Enforcer.

## Enforcer + Engineer

- Bastion et Enforcer peuvent alterner la pression de menace plutôt que cumuler une invulnérabilité.
- Les Ordres affectent le compagnon principal à 75 % et les drones à 40 %.
- Dôme de ferraille protège la nuée contre les dégâts de zone sans la rendre immortelle.
- Ralliement traumatique restaure fortement le compagnon principal, mais pas les drones jetables.

## Marine + Scientist

- Fragilité cristalline augmente les dégâts physiques et explosifs du Marine.
- Les zones thermobariques entretiennent les Brûlures et préparent les Surcharges électriques.
- Armure rompue amplifie les éclatements physiques produits par les cibles gelées.
- Les explosions peuvent détoner Criblé en même temps que les réactions élémentaires, avec une limite par cible pour éviter une récursion infinie.

## Marine + Engineer

- Puits gravimétrique concentre les ennemis dans les mines et roquettes.
- Corrosion et Armure rompue se cumulent selon un plafond global de réduction défensive à définir pendant l'équilibrage.
- Directive Zéro fournit au Marine une cible prioritaire sans modifier sa précision.
- Les explosions Kamikaze peuvent déclencher Amorce et profiter de Déchiqueté si leurs tags de dégâts le permettent.

## Scientist + Engineer

- Noyau interchangeable fait entrer le compagnon dans les réactions élémentaires.
- Champ corrosif réduit les résistances avant Flashover, Fracture cryogénique ou Surcharge.
- Les drones électriques propagent Conductivité ; les Scarabées cryoniques ou thermiques peuvent être créés par équipement spécifique.
- Vapeur de confinement et Puits gravimétrique forment une zone de contrôle idéale pour les essaims.

---

# 7. Exemples de répartitions au niveau 99

Ces répartitions ne sont pas des builds imposés ; elles montrent seulement que 98 points produisent des choix réels.

## Enforcer — Rempart mobile

- Plaques de récupération 20
- Réacteur de représailles 20
- Purge cinétique 20
- Dôme de ferraille 20
- Ancrage d'abattoir 10
- Treuil gravifique 1
- Ordre : Tenez la ligne 1
- Ordre : Avancez 1
- Présence de fer 5

**Total : 98 points.** Défense très forte, Purge alimentée par la Garde et un minimum de commandement.

## Marine — Briseur de ligne

- Munitions à noyau dense 20
- Masse d'arrêt 20
- Ligne de démolition 20
- Surcharge de canon 20
- Percuteur surchargé 7
- Impact sismique 5
- Grenade M-13 3
- Ceinture de démolition 1
- Shrapnel industriel 1
- Réaction en chaîne 1

**Total : 98 points.** Spécialiste lourd capable de détoner quelques Amorce sans devenir un véritable démolisseur.

## Scientist — Réacteur d'orage thermique

- Combustion catalytique 20
- Flashover 20
- Charge conductrice 20
- Singularité électrique 20
- Inciseur thermique 5
- Nappe de prométhium 1
- Propagation pyrolytique 1
- Arc voltaïque 5
- Lance capacitive 4
- Superconduction 1
- Réseau de surtension 1

**Total : 98 points.** Accumule Brûlure et Conductivité, puis combine Flashover et Ionisation.

## Engineer — Nécro-usine

- Bande passante volée 20
- Batteries de charnier 20
- Réplication de terrain 20
- Nuée autonome 20
- Vautours R-4 6
- Scarabées de découpe 6
- Protocole Kamikaze 3
- Essaim de maintenance 1
- Champ corrosif 1
- Algorithme d'exploitation 1

**Total : 98 points.** Multiplie les petites unités, recycle leurs débris et conserve un débuff minimal de soutien.

---

# 8. Règles d'équilibrage recommandées

## 8.1. Plafonds globaux

Les arbres multiplient les sources de réduction et d'amplification. Les plafonds exacts restent à tester, mais les catégories suivantes devraient avoir un plafond commun :

- réduction totale de dégâts reçus ;
- réduction d'armure et de résistances d'une cible ;
- réduction de vitesse d'attaque et de déplacement des ennemis ;
- récupération de recharge par seconde ;
- durée maximale ajoutée à un buff, débuff ou compagnon temporaire ;
- nombre de réactions secondaires qu'une même cible peut produire par seconde.

## 8.2. Boss

- Les boss ne doivent pas ignorer entièrement les builds de contrôle.
- Un gel devient Cryofragile, une provocation devient menace renforcée, un déplacement forcé devient stagger ou vulnérabilité temporaire.
- Les effets sont donc transformés plutôt qu'annulés.
- Les déclenchements fondés sur les éliminations disposent d'une équivalence par tranches de vie retirées aux boss.

## 8.3. Compétences au-delà du rang 20

- Les rangs d'équipement améliorent l'effet direct mais jamais les synergies.
- Les seuils structurels majeurs — nombre de traversées, charges maximales, nouveaux comportements — devraient en principe être obtenus avant ou au rang 20.
- Les rangs 21 et plus augmentent surtout dégâts, durée, rayon, ressource ou recharge selon une courbe contrôlée.
- Cela évite qu'un objet `+5` soit obligatoire pour rendre une compétence fonctionnelle.

## 8.4. Coopération

- Les buffs identiques provenant de plusieurs Enforcers ne se cumulent pas intégralement : le plus puissant s'applique, les suivants peuvent seulement prolonger la durée dans une limite définie.
- Les mêmes débuffs de plusieurs Engineers partagent leurs plafonds de charges.
- Les états élémentaires de plusieurs Scientists peuvent contribuer à la même cible, mais l'attribution des dégâts et des déclenchements doit conserver l'identité de la source.
- Les compagnons et drones doivent respecter un budget visuel, sonore, de collisions et de réseau pour rester lisibles à cinq joueurs.

## 8.5. Lisibilité

Chaque mécanique doit posséder un signal clair sans transformer l'écran en interface abstraite :

- Garde : pression lumineuse dans les vérins ou plaques de l'armure ;
- Carnage : montée sonore et vibrations mécaniques ;
- Saturation : culasse et chargeur chauffés, douilles plus nombreuses ;
- Fracture : fissures, étincelles, plaques arrachées ;
- Amorce : dispositif ou voyant fixé à la cible ;
- Brûlure, Cryostase et Conductivité : effets distincts adaptés à la chair, aux machines et aux entités ;
- Corrosion, Brouillage et Exposition : parasites d'interface, nanites ou balises industrielles ;
- Directive Zéro et Marquage d'extermination : silhouettes et sons prioritaires, sans assistance de visée.

---

# 9. Structure data-driven recommandée

Une compétence peut être décrite par une définition de données indépendante de son exécution :

```text
SkillDefinition
├── Identifiant stable
├── Classe et arbre
├── Nom et description localisés
├── Type : actif / passif
├── Rang naturel maximal : 20
├── Niveau de déblocage
├── Prérequis
├── Tags : mêlée, léger, lourd, explosif, feu, glace, foudre, robot, drone, buff, débuff…
├── Mode de ciblage : soi, réticule, cible, zone curseur, ligne, cône, aura
├── Coût, charges et recharge
├── Courbes par rang
├── Effets et statuts appliqués
├── Règles de consommation de statuts
├── Synergies de points investis
├── Interactions de tags
├── Présentation remplaçable
└── Règles de synchronisation multijoueur
```

Les effets comme Garde, Carnage, Saturation, Fracture, Amorce, Brûlure, Cryostase, Conductivité, Corrosion ou Exposition doivent posséder un identifiant stable et ne jamais être recherchés par leur nom français dans le code.

Les compétences passives s'abonnent aux événements de gameplay pertinents — dégâts appliqués, blocage, élimination, statut ajouté, drone détruit, buff activé — sans appeler directement l'ennemi, la quête ou l'interface.

---

# 10. Ordre de prototypage conseillé

Le document décrit la cible complète, mais la production peut rester compatible avec un projet indépendant :

1. Implémenter un seul arbre de huit compétences, idéalement **Blindage de siège** ou **Doctrine de saturation**.
2. Valider le rang 1, le rang 20 et un rang d'équipement supérieur à 20.
3. Valider une synergie de points investis, une charge consommable et une réaction de statut.
4. Brancher l'affectation aux cinq emplacements actifs.
5. Ajouter un second arbre de la même classe pour tester un véritable build hybride à 98 points.
6. Implémenter ensuite les arbres du Scientist, car leurs réactions valident le système de statuts générique.
7. Terminer par les essaims de l'Engineer, qui demandent le plus de travail d'IA, de lisibilité, de performance et de synchronisation.

Cette progression permet de tester les fondations sans devoir produire immédiatement les 96 compétences, tout en conservant une destination de design cohérente.
