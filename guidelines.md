# Guidelines

## Code

- Noms de méthodes : PascalCase
- Noms d'attributs privés : _camelCase
- Noms d'attributs \[SerializedField\] : PascalCase
- Noms de classes : PascalCase
- Noms d'attributs publics : PascalCase
- Noms de constantes : ALL_UPPERCASE
- Noms d'interfaces : IPascalCase

## Méthodologie de travail dans Unity

- Ne pas travailler dans la même scène qu'un autre (scènes assignées)
- Si vous downloadez un assset pack, ne pas push le asset pack au complet, seulement les assets utilisés

## Méthodologie de travail Git

- Faire une branche pour chaque feature
- La branche devrait être minimalement descriptive de la tache associée
- Ajouter le no de la task sur GitHub dans le nom de la branche
- Quand la feature est terminée faire une PR
- Au moins une personne doit approuver la PR avant de pouvoir merge sur main
- **Ne pas push sur main**
- Quand on review une PR, juste approuver la PR, laisser la personne qui a fait la PR la merge elle-même
- Essayer de faire des noms de commit signifiants
- Dans la PR indiquer le numéro de la tâche (issue) associée de cette façon : "Closes #\<no de tâche\>"

## Méthodologie Kanban (tab Projects dans le repo)

- Avant de commencer une nouvelle tâche, assurez-vous d'être la personne assignée à cette tâche dans le projet
- Assurez-vous de changer l'avancement de vos tâches dans le board (InProgress, Blocked, etc...)
- Avant de créer une tâche feature, assurez-vous que cette feature soit approuvée par le reste de l'équipe
- Si vous travaillez sur une tâche un peu plus longue, assurez d'ajouter des commentaires pour décrire son avancement
- Si votre tâche se retrouve à être bloquée, le communiquer avec le reste de l'équipe soit sur discord ou en taggant la personne dont la tâche vous bloque directement sur GitHub
