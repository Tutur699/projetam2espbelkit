# Projet AM2
*L3 Informatique – Équipe : Belalem Kaisse · Esperança Arthur · Kita Djessy‑Alberto*

## 1. Contexte et objectif  
Ce projet s’inscrit dans le cadre du module « LIFprojet ».  
L’objectif est de développer un **jeu/une application** (sous Unity / C#) lié au thème « AM2 ».  
Il s’agit de mettre en œuvre les compétences acquises en programmation, conception objet, utilisation d’un moteur de jeu, et travail collaboratif.

## 2. Fonctionnalités & périmètre  
### Fonctionnalités principales  
- Mise en place d’une scène de jeu avec environnement, personnage contrôlable, interface utilisateur.  
- Gestion des interactions (IA, ennemis, collisions, récoltes, etc.).  
- Menu principal / pause / score.
- Jeu en réseau.

### Périmètre / limites  
- Version minimale réalisable dans le module (prototype de jeu).  
- Le scope pourra être étendu (par ex. niveaux supplémentaires, sauvegarde etc) selon le temps disponible.  

## 3. Architecture & technologies  
- Moteur : Unity 6.  
- Langage de script : C#.  
- Organisation du projet : dossier `Assets/`, `Packages/`, `ProjectSettings/`.  
- Branches Git : `main` pour la version stable, branches de fonctionnalités pour le développement.  

## 4. Installation & démarrage  
### Prérequis  
- Unity installée.  
- Git pour récupérer le dépôt.  

### Récupérer le projet  
```bash
git clone https://github.com/Tutur699/projetam2espbelkit.git
cd projetam2espbelkit
```

### Ouvrir dans Unity  
1. Lancez Unity Hub → ouvrez le dossier cloné comme projet.  
2. Attendez que le projet soit importé/compilé.  
3. Dans Unity, cliquez sur Ctrl + B (Build & Run)
4. Lancer le .exe

### Build (export)  
- Dans Unity : *File → Build Settings* → sélectionner la plateforme (PC, WebGL…) → Build.  
- Choisir un dossier de destination, puis exécuter le build.

## 5. Organisation du dépôt  
- `Assets/` : scripts, scènes, modèles, textures, audio.  
- `Packages/` : dépendances Unity.  
- `ProjectSettings/` : réglages du projet Unity.  
- `.gitignore` : exclusions Git pour Unity.  
- `README.md` : ce fichier.  
- (Ajouter d’autres dossiers si besoin, par ex. `Docs/`, `Design/`…)

## 6. Contribution & workflow  
- Chaque membre crée une branche nommée `feature/<nom_fonctionnalité>`.  
- Valider des commits fréquents et clairs.  
- Faire des Pull Requests pour fusionner vers `main` après revue.  
- Respecter les conventions de nommage et commenter les scripts.

## 7. État actuel & version  
- Version du prototype : **v0.1**.  

## 8. Auteurs  
- Belalem Kaisse  
- Esperança Arthur  
- Kita Djessy‐Alberto  
> Étudiants en 3ᵉ année Licence Informatique

## 9. Licence  
Ce projet est distribué sous la licence **GNU GPL**.
