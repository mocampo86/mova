# Mova — Backlog priorizado de readiness

Las estimaciones son relativas para un equipo familiarizado con el repositorio: S (hasta pocos días), M (varios días), L (1–2 iteraciones), XL (varias iteraciones). No modifica el backlog existente.

## P0 — Bloqueante de producción

| ID | Hallazgo | Tipo | Est. | Dependencias | Criterios de aceptación resumidos |
|---|---|---|---:|---|---|
| PR-001 | Precio, moneda, snapshot y pago manual | Business/Functional/Data | L | PR-020 | Tarifa base por cancha; total/moneda antes de confirmar; snapshot histórico; estado de pago manual auditado. |
| PR-002 | Comprobante y comunicación transaccional | Functional/Operational | M | PR-001, PR-021 | Reserva/cancelación deja comprobante recuperable con contacto, precio y política; canal mínimo o cola/protocolo de aviso. |
| PR-003 | Idempotencia de reserva puntual | Data Integrity/Technical | M | PR-013 | POST/PATCH puntual exige UUID; replay seguro por actor/operación; SPA conserva clave durante retry; tests. |
| PR-004 | Garantía y prueba de no solapamiento | Data Integrity/Testing | L | PostgreSQL staging | Dos requests simultáneas terminan en un 201 y un 409; DB conserva una; fallos serializables mapean 409; defensa DB evaluada/aplicada. |
| PR-005 | Suite de pruebas no completamente verde | Testing Gap | M | entorno de prueba | Identificar las 4 fallas; ejecución reproducible completamente verde y reporte adjunto al release. |

## P1 — Necesario antes del lanzamiento según alcance

| ID | Hallazgo | Tipo | Est. | Dependencias | Criterios de aceptación resumidos |
|---|---|---|---:|---|---|
| PR-006 | Reprogramar reserva puntual | Functional/UX | M | PR-002–004, decisión precio | Admin reprograma sin solape, preserva auditoría/origen y comunica resultado. |
| PR-007 | Gestionar bloqueos de cancha | Functional/Operational | M | PR-004 | Crear/listar/cancelar bloques; resolver bloqueo versus reserva existente; agenda y disponibilidad coherentes. |
| PR-008 | Modificar futuro de recurrentes desde SPA | Functional/UX | M | decisión actor | UI para propietario/admin, conserva pasado y maneja conflictos. |
| PR-009 | Consola/proceso de aprobación de complejos | Functional/Operational | M | decisión onboarding | Pendientes visibles, activar/desactivar auditado; piloto puede usar procedimiento interno controlado. |
| PR-011 | Condiciones visibles antes de reservar | Business/UX | S | PR-001 | Precio, cancelación, contacto e instrucciones aparecen antes de confirmar. |
| PR-013 | Atomicidad/semántica de idempotencia | Data Integrity | M | PR-003 | Clave concurrente no crea duplicados ni genera 500; TTL/cleanup y errores definidos. |
| PR-014 | Reloj inyectable para reglas temporales | Technical Debt/Testing | S | — | Cancelación/historial/dashboard son deterministas bajo reloj de prueba. |
| PR-015 | Pipeline de despliegue a staging/producción | Operational | L | Azure/decisión cloud | artefacto inmutable, gates, smoke, migración respaldada, rollback documentado/ensayado. |
| PR-016 | Evidencia de backup/restore y configuración productiva | Operational/Security | M | Azure | restauración de backup a entorno no prod, smoke post-restore, checklist Key Vault/CORS/TLS/dominio. |
| PR-017 | Gestión de sesión/rotación para público | Security | M | decisión auth | expiración, logout/refresh/revocación definidos y probados; piloto puede aceptar token corto sin refresh. |

## P2 — Importante después del piloto

| ID | Hallazgo | Tipo | Est. | Recomendación |
|---|---|---|---:|---|
| PR-010 | Gestión de administradores adicionales | Functional | M | Invitación/revocación, último admin protegido. |
| PR-012 | Check-in y motivo de no-show | Operational | M | Mejorar recepción e indicadores. |
| PR-018 | Host/CORS/configuración productiva declarativa | Security/Operational | S | Host filtering, origins, headers y prueba staging. |
| PR-019 | Rendimiento, retención y capacidad | Technical/Operational | M | Carga de disponibilidad, retención audit/idempotency, SLO. |
| PR-020 | Fechas presentadas en timezone del complejo | UX/UI | S | Formatear agenda/comprobantes con IANA del complejo. |
| PR-021 | Accesibilidad y pruebas móviles | UX/UI/Testing | M | axe, teclado, lectores y dispositivos representativos. |
| PR-022 | Tarifas por franja/día y feriados | Business | L | Evolucionar reglas sin romper snapshots. |
| PR-023 | Email robusto/outbox y reintentos | Operational | M | Observabilidad y reintentos de notificación. |

## P3 — Evolución futura

- Pago online, depósitos, reembolsos automáticos e integración contable/facturación.
- Promociones, descuentos complejos, precios dinámicos y membresías.
- WhatsApp/SMS/push, CRM/marketing, reputación, torneos y apps nativas.
- Multi-región, autoscaling avanzado y separación de servicios.

## Orden recomendado

1. **PR-005**, para contar con una línea base confiable.
2. En paralelo: **PR-001 + PR-011** y **PR-003 + PR-004**.
3. **PR-002, PR-007, PR-006**: operación comercial diaria.
4. **PR-008, PR-009, PR-015, PR-016** antes de abrir el piloto con autonomía limitada.
5. P2 según evidencia del piloto; P3 sólo si respalda una hipótesis comercial validada.
