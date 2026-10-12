# Mova — Roadmap de lanzamiento

## Fase 0 — Decisiones y baseline (antes de construir)

1. Aprobar modelo comercial mínimo: moneda, tarifa base, snapshot, pago manual y tratamiento de precio al reprogramar.
2. Definir si el piloto requiere email o comprobante descargable/protocolo manual; definir responsable de comunicación.
3. Resolver las cuatro fallas de `dotnet test` y capturar una ejecución reproducible.
4. Elegir complejos piloto, zona geográfica/moneda, horario de soporte y responsable de aprobación de complejos.

**Salida:** decisiones de `07-OPEN-DECISIONS.md` aprobadas y suite base verde.

## Fase 1 — Núcleo seguro y comercial (P0)

- PR-001, PR-011: precios simples, moneda, snapshot de la reserva y estado de pago manual.
- PR-003, PR-013: idempotencia de mutaciones puntuales.
- PR-004: test real de concurrencia, mapeo seguro de conflicto y última línea de defensa de DB según diseño seleccionado.
- PR-002: comprobante/confirmación e información operativa.

**Salida:** el jugador entiende qué reserva y cuánto debe; el complejo puede registrar cobro sin planilla; no hay doble turno por retry/concurrencia demostrada.

## Fase 2 — Operación de recepción (P1)

- PR-007: bloques de cancha manejables.
- PR-006: reprogramación puntual con trazabilidad.
- PR-008: modificación futura de recurrentes o des-scope explícito.
- PR-009: flujo controlado de aprobación de complejos.
- Pruebas E2E reales: crear, cancelación por policy, manual, bloque, reprogramación, recurrente y pago manual.

**Salida:** el administrador puede sostener una jornada sin inventar procesos críticos fuera de Mova.

## Fase 3 — Preparación del piloto

- PR-015/016: staging, Key Vault, CORS/dominio/TLS, Azure PostgreSQL, alertas Bicep desplegadas, backup + restore drill, runbook y rollback probado.
- Crear datos de prueba y ejecutar los escenarios de `06-END-TO-END-SCENARIOS.md` con personal del complejo.
- Capacitar al equipo piloto: tarifas, reservas presenciales, cancelaciones, no-show, pagos manuales e incidentes.

### Escenario A — Piloto controlado

**Alcance mínimo:** 1–3 complejos, una moneda/país, tarifa única por cancha, pago presencial/manual, soporte cercano, aprobación manual de complejos y comunicación mínima definida.

**Controles requeridos:** todos los P0 resueltos; P1 PR-006/007 resueltos; otros P1 con workaround documentado y owner; monitoreo/backup/restauración verificables; límite de volumen y canal de incidentes.

**Riesgo aceptable:** ausencia de precios por franja, promociones, integración de pagos y gestión autónoma completa de admins. No es aceptable perder/duplicar reserva ni ocultar precio/condiciones.

## Fase 4 — Lanzamiento público

Además del piloto estable, exige:

- Todos los P1 resueltos, en particular onboarding/autorización de complejos y administración de personal.
- Gestión de sesión/rotación y una política de soporte/privacidad/retención aprobada.
- Pipeline automatizado de despliegue con migraciones controladas, smoke, rollback y trazabilidad de versión.
- E2E integrado contra entorno real, carga/capacidad de disponibilidad y alertas/guardia operativa.
- Accesibilidad y experiencia móvil validadas; comunicación transaccional confiable con reintentos.
- Métricas de producto/operación: tasa de reserva, conflictos, cancelaciones/no-show, errores, latencia y entregabilidad.

## Criterio de no-go

No abrir reservas reales si cualquiera de estos puntos no está demostrado: tarifa/condición visible e histórica, una sola reserva activa por slot bajo concurrencia, mutaciones reintentables seguras, suite crítica verde, backup restaurable, autenticación/configuración productiva segura y capacidad de comunicar el comprobante al cliente.
