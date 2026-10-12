# Mova — Diagnóstico técnico de preparación

## Backend y datos

### Fortalezas verificadas
- Capas Domain/Application/Infrastructure/API y tests de arquitectura; `dotnet build Mova.slnx --no-restore` terminó correctamente.
- Reserva y recurrencia usan `IsolationLevel.Serializable`: `CreateReservationHandler.cs:77-123`, `CreateRecurringReservationHandler.cs:35-89`.
- Disponibilidad usa zona IANA y UTC; las reglas locales se convierten a UTC: `GetCourtAvailabilityHandler.cs:38-66`.
- Autorización compleja consulta asociación activa en base de datos: `ComplexAdminAuthorizationHandler.cs:45-53`, `ComplexAdministratorRepository.cs:25-31`.
- Error global, logging JSON/redacción, correlation ID, rate limiting y health checks están cableados en `Program.cs:34-76,186-269`.

### Riesgos y brechas

| ID | Categoría | Prioridad | Evidencia / impacto | Recomendación |
|---|---|---:|---|---|
| PR-003 | Data Integrity Risk | P0 | La API design exige idempotencia en mutaciones (`API-DESIGN.md:11`), pero los cinco mutadores puntuales de `ReservationsController.cs:75-232` no usan `IdempotencyRequired`; la SPA tampoco envía clave en `reservationApi.ts:106-172,334-438`. | Aplicar filtro/store atómico a crear/cancelar/status y conservar clave para retry. |
| PR-004 | Data Integrity Risk | P0 | `ReservationConfiguration.cs:25-31` sólo tiene índice + check temporal; no hay exclusion constraint PostgreSQL. El control es consulta de solape + transacción. No hay `Task.WhenAll` en pruebas. | Prueba concurrente real; traducir serialization failure a 409; agregar defensa DB (exclusion constraint/trigger) evaluada para estados activos. |
| PR-013 | Testing Gap | P1 | Las cancelaciones y cambios de estado de reserva puntual no son idempotentes y la implementación del store persiste después de la acción, sin coordinación de solicitudes con misma clave (`IdempotencyRequiredAttribute.cs:39-60`, `IdempotencyStore.cs:28-46`). | Diseñar semántica de clave concurrente y respuestas de error; probar replay/concurrencia/TTL. |
| PR-014 | Technical Debt | P1 | No se halló `TimeProvider` consistente: varios handlers usan `DateTime.UtcNow` (`grep` en Application). | Inyectar reloj para reglas de deadline e historial, elevar determinismo de pruebas. |
| PR-015 | Operational Risk | P1 | No hay pipeline de deployment; GitHub Actions sólo corre PR y no E2E (`.github/workflows/ci.yml:3-112`). Bicep sólo configura monitoring sobre recursos existentes (`devops/azure/main.bicep:1-56`). | Pipeline staging/prod con aprobación, build artefacto, smoke, migración controlada y rollback. |
| PR-016 | Operational Risk | P1 | Estrategia backup/restore está documentada, pero no hay evidencia de restore drill ejecutado ni IaC de DB/Key Vault/red. | Checklist firmado de backup/restore pre-piloto y automatización/monitor para público. |
| PR-017 | Security Risk | P1 público | JWT HMAC simétrico sin refresh/revocación ni logout server-side (`JwtTokenService.cs:29-72`); tokens en memoria reducen persistencia pero un acceso robado vive 15 min. | Mantener 15 min para piloto; decidir refresh/revocación y rotación de llave para público. |
| PR-018 | Technical Debt | P2 | `AllowedHosts: "*"` en `appsettings.json:8`; CORS seguro por defecto si no hay origen, pero configuración productiva no está versionada. | Definir dominio/orígenes, host filtering y prueba de configuración por ambiente. |
| PR-019 | Performance/Operational Risk | P2 | El calendario solicita disponibilidad por cancha y fecha; sin caching ni pruebas de carga. No hay política de retención de auditoría/idempotencia. | Medir piloto, índices/plan de queries, límites/cleanup y SLOs antes de escalar. |

## Frontend, UX y accesibilidad

- **Implementado:** layouts responsive con drawer, MUI, traducciones, estados loading/error/empty en páginas principales, rutas protegidas (`router.tsx`, `ComplexAdminLayout.tsx`).
- **Parcial:** protección de perfil completo depende de `UserLayout` y estado en memoria; tras refresh el token no se persiste, lo cual es seguro pero implica re-login y recuperación pobre de sesión. `AuthProvider.tsx:19-52`.
- **PR-011/P1:** la reserva no comunica precio, política ni requisitos en el diálogo (`ComplexDetailPage.tsx:81-119`).
- **PR-020/P2:** fechas se renderizan usando el timezone del navegador en varias páginas (`toLocaleString`), aunque el complejo usa IANA. Un admin remoto puede interpretar mal una reserva. Usar timezone del complejo en agenda y comprobantes.
- **PR-021/P2:** revisión estática no encontró bloqueo de accesibilidad crítico; MUI aporta semántica, pero faltan auditoría automatizada (axe), prueba teclado y móvil físico.

## Seguridad

- Google ID token se valida backend y JWT se valida issuer/audience/lifetime/firma (`Program.cs:106-128`); claves vacías fallan por validación de opciones (`DependencyInjection.cs:58-68`).
- Autorización por complejo y pruebas cross-tenant existen; no se verificó una falla de aislamiento.
- Rate limiting definido para login/reservas/rebúsqueda, con proxy confiable configurable (`Program.cs:186-217`).
- Riesgo operativo: no hay evidencia de configuración Azure de `KnownProxies`, CORS, Key Vault, TLS/dominio o secretos. Estos son precondiciones de despliegue, no afirmaciones de defecto en el código.

## Infraestructura

| Área | Piloto | Público |
|---|---|---|
| HTTPS/dominio/CORS/Key Vault | Obligatorio y validado manualmente | Obligatorio automatizado, rotación y acceso mínimo |
| Azure PostgreSQL + backups | Obligatorio, backup y restore probado una vez | HA/geo-redundancia/retención y drills periódicos |
| Observabilidad | App Insights, alertas Bicep desplegadas y receptor probado | dashboards, SLO/alertas de capacidad y on-call |
| CI/CD | artefacto versionado, staging, aprobación manual, migración respaldada | pipeline declarativo, slots/canary, rollback ensayado |
| Escala | una instancia puede servir piloto | pruebas carga, estrategia multiinstancia y métricas fleet-wide |

## Calidad y verificaciones ejecutadas

| Comando | Resultado |
|---|---|
| `dotnet build Mova.slnx --no-restore` | Correcto: 0 warnings, 0 errores. |
| `dotnet test Mova.slnx --no-build --verbosity normal` | **No correcto:** 158 total, 154 correctas, 4 incorrectas; se ejecutó contra `mova_test` local. Requiere triage de fallos antes de liberar. |
| `npm run lint` | Correcto (exit 0). |
| `npm run build` | Correcto (exit 0). |
| `npx vitest run --pool=threads` | Correcto (exit 0). |
| `az bicep build --file devops/azure/main.bicep` | Correcto (exit 0, con warning de configuración CLI). |

### Cobertura faltante crítica
- Concurrencia real de reserva/recurrencia contra PostgreSQL: no encontrada.
- Idempotencia de reserva puntual: no encontrada.
- E2E contra API real para reservar, cancelar, pago manual, bloqueo y reprogramación: no realizada.
- E2E existe para disponibilidad y recurrentes, pero el recurrente intercepta API con mocks (`src/mova-web/e2e/epic-07-recurring.e2e-spec.ts:20-128`), por lo que no valida backend real.
- Seguridad: faltan SAST/dependency scanning explícitos, prueba de headers/TLS/CORS en staging y pruebas de secretos/configuración.
