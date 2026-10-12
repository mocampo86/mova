# Mova — Escenarios end-to-end obligatorios

Cada escenario debe probarse contra PostgreSQL real y una SPA integrada (no sólo mocks), registrando versión, ambiente, evidencia y resultado.

| ID | Escenario | Pasos y resultado esperado | Prioridad |
|---|---|---|---:|
| E2E-01 | Reserva pública con precio | Descubrir complejo, cancha, fecha; ver tarifa/moneda, cancelación, contacto; reservar; recibir comprobante. Total almacenado igual al mostrado. | P0 |
| E2E-02 | Doble intento/reintento | Simular timeout tras POST y reenviar misma clave. Debe devolver el mismo resultado, sin reserva adicional. | P0 |
| E2E-03 | Dos jugadores mismo slot | Dos clientes autenticados envían simultáneamente slot idéntico. Uno recibe 201, otro 409; DB contiene una activa. | P0 |
| E2E-04 | Pago presencial/manual | Admin crea o abre reserva, registra Unpaid/Paid y referencia; historial conserva importe/moneda/actor. Cambiar tarifa no modifica reserva previa. | P0 |
| E2E-05 | Cancelación de jugador | Antes/después del deadline y con policy deshabilitada. Estado, actor, motivo, disponibilidad y comunicación correctos. | P0 |
| E2E-06 | Reserva telefónica/presencial | Admin busca/identifica cliente, crea reserva con horario local y pago manual; aparece en agenda y para jugador. | P1 |
| E2E-07 | Bloqueo de mantenimiento | Admin bloquea cancha futura; no aparece disponible; intento manual/público falla. Definir tratamiento si ya había reserva. | P1 |
| E2E-08 | Reprogramación puntual | Admin mueve turno a slot libre, conserva historial, no solapa, actualiza condiciones/pago según regla y notifica. | P1 |
| E2E-09 | Serie recurrente | Usuario/admin crea serie en zona horaria; DST y bloque/conflicto se validan; cancela una ocurrencia y serie completa. | P1 |
| E2E-10 | Cambio futuro de recurrente | Admin cambia día/hora desde fecha efectiva; pasado inalterado y futuro regenerado sin conflictos. | P1 |
| E2E-11 | Aislamiento multi-complejo | Admin A no ve ni modifica datos B; usuario sólo consulta sus propias reservas; asociaciones inactivas rechazan acceso. | P0 |
| E2E-12 | Onboarding/activación | Propietario registra complejo, queda Pending, operador lo aprueba, configura cancha/horarios/tarifa y se hace visible. | P1 |
| E2E-13 | Bloqueo de usuario | Admin bloquea temporalmente; jugador ve estado y no reserva ese complejo, pero puede reservar otro; desbloqueo restaura acceso. | P1 |
| E2E-14 | Fallo y recuperación | API/DB falla; readiness/alerta se disparan, rollback o restore se ejecuta según runbook y reservas/auditoría permanecen íntegras. | P1 |
| E2E-15 | Móvil y accesibilidad | Completar E2E-01/05/06 en viewport móvil, teclado y lector/auditoría axe; sin tablas/acciones inaccesibles. | P2 |

## Criterios transversales

- Fechas y horas se muestran en la zona del complejo, no sólo la del navegador.
- Todos los errores tienen mensaje accionable y trace/correlation ID no sensible.
- Las acciones administrativas sensibles quedan en auditoría.
- Cada caso incluye estados loading, error, retry y actualización de disponibilidad/agenda.
- Un test E2E no sustituye unit/integration: concurrencia e integridad deben verificarse también a nivel HTTP/DB.
