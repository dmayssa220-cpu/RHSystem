# Interface web SIRH

Application Angular 21 + PrimeNG 21 + Tailwind CSS 4, construite à partir du template
gratuit **Sakai-ng** (PrimeTek, licence MIT — voir `LICENSE.md`).

- Template d'origine : https://github.com/primefaces/sakai-ng, commit `96d71496d685b5c110efd2875abaa2bf89a56ad2`
  (ressources : https://github.com/cetincakiroglu/sakai-assets, commit `eaa70ece4cfa9aeb8f60b36dce5a422b6bae8003`).
  Les deux ont été fusionnés dans ce dossier (le template utilise un sous-module git, qu'on n'a pas conservé).

## Personnalisations déjà faites

- Interface en français uniquement (`lang="fr"`, traductions PrimeNG dans `src/app/core/primeng-fr.ts`, locale `fr-TN`).
- Couleur primaire indigo (`src/app.config.ts`), police Inter **auto-hébergée** (aucune ressource externe).
- Menu et routes pilotés par `src/app/core/modules.ts` : ajouter un module = ajouter une entrée.
- Pipe `tnd` (dinar tunisien, 3 décimales) : `{{ montant | tnd }}`.
- Build de production sans script ni style inline dans `index.html` (compatible CSP stricte).
- Les pages de démonstration du template sont conservées sous « Références UI (démo) » pour t'inspirer ;
  elles seront supprimées avant la mise en production.

## Commandes (optionnel)

Le lancement normal du projet se fait avec `docker compose up --build` depuis la racine du
dépôt — rien à installer ici. Les commandes ci-dessous ne servent que si tu veux travailler
sur l'interface avec rechargement à chaud, en dehors de Docker :

```bash
npm ci          # installer les dépendances
npm start       # serveur de développement sur http://localhost:4200
npm run build   # build de production dans dist/sakai-ng/browser
```
