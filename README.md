# Cas particulier : Concours Ubisoft
Pour l'instant vide. On attends le thème.
On ne peut pas completer les jalons car nous attendons le thème.


## Commande pour export les issues GitHub en CSV

(
  echo "title,description"
  gh issue list --limit 1000 --state all \
    --json title,body \
    --jq '.[] | [.title, .body] | @csv'
) > issues.csv

