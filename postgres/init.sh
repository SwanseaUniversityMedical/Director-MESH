#!/bin/bash
# Creates a dedicated database for Keycloak on first postgres initialisation.
# The director_mesh user is a superuser (POSTGRES_USER), so it can create databases.
set -e
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE DATABASE keycloak;
    GRANT ALL PRIVILEGES ON DATABASE keycloak TO $POSTGRES_USER;
EOSQL
