# Backup and Restore Procedures

This document describes the backup and restore procedures for the Mova platform.

## Scope

Backups cover the single shared PostgreSQL database used by the Mova API. All tenant data is logically isolated by `SportsComplexId` inside this database.

## Local development

### Backup

Use `pg_dump` to create a logical backup of the local `mova` database:

```powershell
$env:PGPASSWORD = "YourStrongPasswordHere"
pg_dump -h localhost -p 5432 -U postgres -d mova -F c -f mova-local-$(Get-Date -Format yyyyMMddHHmmss).dump
```

### Restore

1. Ensure the target database exists. Create it if necessary:

   ```powershell
   psql -h localhost -p 5432 -U postgres -c "CREATE DATABASE mova;"
   ```

2. Restore from the backup file:

   ```powershell
   $env:PGPASSWORD = "YourStrongPasswordHere"
   pg_restore -h localhost -p 5432 -U postgres -d mova --no-owner --no-privileges <backup-file>.dump
   ```

3. Apply any pending EF Core migrations:

   ```powershell
   dotnet ef database update --project src/Mova.Infrastructure --startup-project src/Mova.Api
   ```

## Azure Database for PostgreSQL Flexible Server

### Automated backups

- Configure **automated backups** through the Azure portal or Azure CLI.
- Use the **geo-redundant backup** option for production to protect against regional failures.
- Set the retention period according to the compliance and recovery requirements (minimum 7 days, recommended 35 days for production).

### Manual backup

Create a point-in-time backup before destructive operations such as major migrations or bulk data changes:

```powershell
az postgres flexible-server backup create \
  --resource-group <resource-group> \
  --name <server-name> \
  --backup-name mova-pre-migration-$(Get-Date -Format yyyyMMddHHmmss)
```

### Restore

Restore to a new server from an automated backup or to a point in time:

```powershell
az postgres flexible-server restore \
  --resource-group <resource-group> \
  --name <new-server-name> \
  --source-server <source-server-name> \
  --restore-point-in-time "2026-08-28T00:00:00Z"
```

After restoring, update the API connection string to point to the new server and restart the App Service or container.

## Testing restore procedures

- Restore a backup to a non-production environment monthly.
- Run the integration test suite against the restored database.
- Verify that audit logs, reservations, and complex data are consistent after restore.
- Document any issues or deviations found during the test.

## Backup retention

| Environment | Retention | Storage |
|-------------|-----------|---------|
| Local | Manual, kept by developer | Developer machine |
| Dev/QA | 7 days | Azure automated backups or container volumes |
| Staging | 14 days | Azure automated backups |
| Production | 35 days, geo-redundant | Azure automated backups with long-term retention |

## Responsibilities

- **DevOps Engineer**: configure automated backups, monitor backup health, and execute restore drills.
- **Backend Engineer**: ensure migrations are reversible or paired with a rollback script before production deployment.
- **QA Engineer**: verify the restored environment with smoke tests and integration tests.
