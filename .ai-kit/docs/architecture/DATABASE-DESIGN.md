# Database Design

## Overview

- **Engine**: PostgreSQL 15+
- **Topology**: Single shared database for the MVP.
- **Isolation strategy**: logical row-level isolation through `SportsComplexId`.
- **ORM**: Entity Framework Core.
- **Migrations**: managed via EF Core command-line tools and versioned in source control.

## Schema

All tables live in the default `public` schema unless a future decision introduces schema-per-bounded-context.

## Tables

### Users

```sql
CREATE TABLE Users (
    Id UUID PRIMARY KEY,
    GoogleSubjectId TEXT NOT NULL UNIQUE,
    Email TEXT NOT NULL UNIQUE,
    FullName TEXT NOT NULL,
    PhoneNumber TEXT NOT NULL,
    PhoneVerified BOOLEAN NOT NULL DEFAULT FALSE,
    Status TEXT NOT NULL,
    CreatedAt TIMESTAMPTZ NOT NULL,
    UpdatedAt TIMESTAMPTZ
);
```

### SportsComplexes

```sql
CREATE TABLE SportsComplexes (
    Id UUID PRIMARY KEY,
    Name TEXT NOT NULL,
    Description TEXT,
    Address TEXT NOT NULL,
    City TEXT NOT NULL,
    Latitude NUMERIC,
    Longitude NUMERIC,
    PhoneNumber TEXT,
    Email TEXT,
    Status TEXT NOT NULL,
    AllowUserRecurringReservations BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMPTZ NOT NULL,
    UpdatedAt TIMESTAMPTZ
);
```

### ComplexAdministrators

```sql
CREATE TABLE ComplexAdministrators (
    Id UUID PRIMARY KEY,
    SportsComplexId UUID NOT NULL REFERENCES SportsComplexes(Id),
    UserId UUID NOT NULL REFERENCES Users(Id),
    Role TEXT NOT NULL,
    Status TEXT NOT NULL,
    CreatedAt TIMESTAMPTZ NOT NULL,
    UNIQUE (SportsComplexId, UserId)
);
```

### Sports

```sql
CREATE TABLE Sports (
    Id UUID PRIMARY KEY,
    Name TEXT NOT NULL UNIQUE,
    Status TEXT NOT NULL
);
```

### Courts

```sql
CREATE TABLE Courts (
    Id UUID PRIMARY KEY,
    SportsComplexId UUID NOT NULL REFERENCES SportsComplexes(Id),
    Name TEXT NOT NULL,
    Description TEXT,
    SurfaceType TEXT,
    Indoor BOOLEAN,
    Status TEXT NOT NULL,
    CreatedAt TIMESTAMPTZ NOT NULL,
    UpdatedAt TIMESTAMPTZ
);
```

### CourtSports

```sql
CREATE TABLE CourtSports (
    CourtId UUID NOT NULL REFERENCES Courts(Id) ON DELETE CASCADE,
    SportId UUID NOT NULL REFERENCES Sports(Id) ON DELETE CASCADE,
    PRIMARY KEY (CourtId, SportId)
);
```

### BusinessHours

Opening and closing times may wrap past midnight (e.g. 22:00–02:00). The table does not enforce `OpeningTime < ClosingTime` so overnight hours are supported.

```sql
CREATE TABLE BusinessHours (
    Id UUID PRIMARY KEY,
    SportsComplexId UUID NOT NULL REFERENCES SportsComplexes(Id),
    DayOfWeek INT NOT NULL CHECK (DayOfWeek BETWEEN 0 AND 6),
    OpeningTime TIME NOT NULL,
    ClosingTime TIME NOT NULL,
    IsClosed BOOLEAN NOT NULL DEFAULT FALSE,
    UNIQUE (SportsComplexId, DayOfWeek)
);
```

### CourtAvailabilityRules

```sql
CREATE TABLE CourtAvailabilityRules (
    Id UUID PRIMARY KEY,
    CourtId UUID NOT NULL REFERENCES Courts(Id),
    DayOfWeek INT NOT NULL CHECK (DayOfWeek BETWEEN 0 AND 6),
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    SlotDurationMinutes INT NOT NULL DEFAULT 60,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT chk_time_order CHECK (StartTime <> EndTime)
);
```

### Reservations

```sql
CREATE TABLE Reservations (
    Id UUID PRIMARY KEY,
    SportsComplexId UUID NOT NULL REFERENCES SportsComplexes(Id),
    CourtId UUID NOT NULL REFERENCES Courts(Id),
    UserId UUID NOT NULL REFERENCES Users(Id),
    StartAt TIMESTAMPTZ NOT NULL,
    EndAt TIMESTAMPTZ NOT NULL,
    Status TEXT NOT NULL,
    Source TEXT NOT NULL,
    RecurringReservationId UUID,
    Notes TEXT,
    CreatedAt TIMESTAMPTZ NOT NULL,
    UpdatedAt TIMESTAMPTZ,
    CancelledAt TIMESTAMPTZ,
    CancellationReason TEXT,
    CancelledByUserId UUID REFERENCES Users(Id),
    CONSTRAINT chk_reservation_time CHECK (StartAt < EndAt)
);
```

### RecurringReservations

```sql
CREATE TABLE RecurringReservations (
    Id UUID PRIMARY KEY,
    SportsComplexId UUID NOT NULL REFERENCES SportsComplexes(Id),
    CourtId UUID NOT NULL REFERENCES Courts(Id),
    UserId UUID NOT NULL REFERENCES Users(Id),
    DayOfWeek INT NOT NULL CHECK (DayOfWeek BETWEEN 0 AND 6),
    StartTime TIME NOT NULL,
    DurationMinutes INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Status TEXT NOT NULL,
    CreatedAt TIMESTAMPTZ NOT NULL,
    UpdatedAt TIMESTAMPTZ,
    CONSTRAINT chk_recurring_dates CHECK (StartDate <= EndDate)
);
```

### CourtBlocks

```sql
CREATE TABLE CourtBlocks (
    Id UUID PRIMARY KEY,
    SportsComplexId UUID NOT NULL REFERENCES SportsComplexes(Id),
    CourtId UUID NOT NULL REFERENCES Courts(Id),
    StartAt TIMESTAMPTZ NOT NULL,
    EndAt TIMESTAMPTZ NOT NULL,
    Reason TEXT,
    CreatedByUserId UUID NOT NULL REFERENCES Users(Id),
    CreatedAt TIMESTAMPTZ NOT NULL,
    CONSTRAINT chk_block_time CHECK (StartAt < EndAt)
);
```

### BlockedUsers

```sql
CREATE TABLE BlockedUsers (
    Id UUID PRIMARY KEY,
    SportsComplexId UUID NOT NULL REFERENCES SportsComplexes(Id),
    UserId UUID NOT NULL REFERENCES Users(Id),
    Reason TEXT,
    BlockedAt TIMESTAMPTZ NOT NULL,
    BlockedUntil TIMESTAMPTZ,
    BlockedByUserId UUID NOT NULL REFERENCES Users(Id),
    Status TEXT NOT NULL,
    UNIQUE (SportsComplexId, UserId, Status)
);
```

### AuditLogs

```sql
CREATE TABLE AuditLogs (
    Id UUID PRIMARY KEY,
    UserId UUID REFERENCES Users(Id),
    SportsComplexId UUID REFERENCES SportsComplexes(Id),
    Action TEXT NOT NULL,
    EntityType TEXT NOT NULL,
    EntityId TEXT NOT NULL,
    CreatedAt TIMESTAMPTZ NOT NULL,
    Metadata TEXT
);
```

## Critical indexes

```sql
-- Multi-tenancy filtering
CREATE INDEX IX_Courts_SportsComplexId ON Courts (SportsComplexId);
CREATE INDEX IX_Reservations_SportsComplexId ON Reservations (SportsComplexId);
CREATE INDEX IX_BlockedUsers_SportsComplexId ON BlockedUsers (SportsComplexId);

-- Availability and conflict detection
CREATE INDEX IX_Reservations_CourtId_StartAt_EndAt_Status
ON Reservations (CourtId, StartAt, EndAt, Status);

CREATE INDEX IX_CourtBlocks_CourtId_StartAt_EndAt
ON CourtBlocks (CourtId, StartAt, EndAt);

-- Lookup by user
CREATE INDEX IX_Reservations_UserId ON Reservations (UserId);
CREATE INDEX IX_Reservations_CancelledByUserId ON Reservations (CancelledByUserId);
CREATE INDEX IX_Users_GoogleSubjectId ON Users (GoogleSubjectId);
CREATE INDEX IX_Users_Email ON Users (Email);

-- Audit
CREATE INDEX IX_AuditLogs_SportsComplexId_CreatedAt
ON AuditLogs (SportsComplexId, CreatedAt DESC);
```

## Concurrency and conflict prevention

Reservation creation and recurring reservation generation must run inside a database transaction. The recommended strategy to prevent overlapping reservations is a combination of:

1. **Range overlap query** with a pessimistic condition in application code.
2. **Serializable transaction isolation** or **advisory lock** per `(CourtId, StartAt)` during creation.
3. **Unique filtered index** where supported, or a **constraint trigger** as a last-line defense.

Example overlap check:

```sql
SELECT 1
FROM Reservations
WHERE CourtId = :courtId
  AND Status NOT IN ('CancelledByUser', 'CancelledByAdmin')
  AND StartAt < :requestedEnd
  AND EndAt > :requestedStart;
```

## Migrations

- Each model change requires an EF Core migration.
- Migration names must be descriptive, e.g. `InitialSchema`, `AddPhoneVerified`, `AddCourtBlockReason`.
- Migrations must be tested against a PostgreSQL container before merging.
- Never modify a migration that has already been applied to production unless a rollback script is provided.

## Seeding

Initial seed data for `Sports` should be applied through a dedicated data seed migration or an idempotent `DbSeeder` utility invoked at startup in development environments.

## Backup strategy

- Daily automated backups in production.
- Test restore procedure monthly in a non-production environment.
