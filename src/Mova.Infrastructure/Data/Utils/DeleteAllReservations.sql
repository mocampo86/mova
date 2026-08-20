-- DeleteAllReservations.sql
-- WARNING: This script permanently deletes all reservation data.
-- Only run this in local development or when you explicitly intend to reset reservation state.
--
-- Removes:
--   - All single reservations from the "Reservations" table.
--   - All recurring reservation plans from the "RecurringReservations" table.
--
-- Usage with psql (example):
--   psql -h localhost -U postgres -d mova -f src/Mova.Infrastructure/Data/Utils/DeleteAllReservations.sql
--
-- "Reservations" is deleted first because it references "RecurringReservations".
-- If you only want to remove single reservations, comment out the second DELETE statement.

BEGIN;

DELETE FROM "Reservations";
DELETE FROM "RecurringReservations";

COMMIT;
