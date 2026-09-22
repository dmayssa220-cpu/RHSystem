#!/bin/bash
# Exécuté une seule fois, à la première création du volume MySQL.
# Crée deux comptes distincts (principe du moindre privilège) :
#  - migrator : droits complets sur la base, réservé aux migrations de schéma
#  - app      : droits de lecture/écriture de données uniquement (pas de DDL)
set -euo pipefail

mysql -uroot -p"${MYSQL_ROOT_PASSWORD}" <<SQL
CREATE USER IF NOT EXISTS '${MYSQL_MIGRATOR_USER}'@'%' IDENTIFIED BY '${MYSQL_MIGRATOR_PASSWORD}';
GRANT ALL PRIVILEGES ON \`${MYSQL_DATABASE}\`.* TO '${MYSQL_MIGRATOR_USER}'@'%';

CREATE USER IF NOT EXISTS '${MYSQL_APP_USER}'@'%' IDENTIFIED BY '${MYSQL_APP_PASSWORD}';
GRANT SELECT, INSERT, UPDATE, DELETE ON \`${MYSQL_DATABASE}\`.* TO '${MYSQL_APP_USER}'@'%';

FLUSH PRIVILEGES;
SQL
