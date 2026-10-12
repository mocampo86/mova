# Mova — Análisis de brechas funcionales

## Convenciones de evidencia

- **Inexistente:** no se encontró entidad, contrato, endpoint ni UI.
- **Parcial:** existe una porción del flujo, pero no permite completar la operación.
- **Implementada/no probada:** se observó código sin prueba relevante de extremo a extremo o concurrencia.
- **Implementada y validada:** código y pruebas pertinentes observados.

## Hallazgos funcionales

### PR-001 — Precio, moneda y pago manual ausentes
- **Categoría / prioridad:** Business Gap, Functional Gap, Data Integrity Risk — **P0**.
- **Estado:** Inexistente.
- **Problema y escenario:** un jugador elige cancha y horario, confirma, pero no sabe cuánto pagará ni cómo/si fue registrado el cobro. El administrador no puede cerrar caja ni mantener el importe acordado si la tarifa cambia.
- **Actual / esperado:** `Reservation` sólo persiste cancha, usuario, horario, estado y notas; el diálogo de reserva sólo muestra horario. Debe persistir y mostrar precio total, moneda y estado de pago manual como snapshot histórico.
- **Evidencia:** `src/Mova.Domain/Entities/Reservation.cs:7-29`; `src/Mova.Contracts/Reservations`; `src/mova-web/src/pages/ComplexDetailPage.tsx:81-119`; búsqueda repositorio sin `Price|Payment|Currency`. La documentación declara pricing/payments como extensión futura en `.ai-kit/docs/architecture/DOMAIN-MODEL.md:249-253`.
- **Impacto:** imposibilita una operación comercial clara aun sin pasarela de pago.
- **Recomendación:** tarifa única por cancha/slot, moneda por complejo, snapshot de total/currency en reserva y pago manual auditado. Complejidad **L**. Depende de decisión comercial PR-020.
- **Aceptación:** precio visible antes de confirmar; total inmutable en reservas históricas; cambio de tarifa sólo para nuevas reservas; administrador puede registrar estado y referencia de pago; pruebas de cálculo e historial.

### PR-002 — Sin confirmación ni comunicación operativa
- **Categoría / prioridad:** Functional Gap, Operational Risk — **P0**.
- **Estado:** Inexistente.
- **Escenario:** tras confirmar/cancelar/reprogramar, cliente y complejo deben saber qué ocurrió, dirección, contacto, horario local, precio y condiciones. Hoy un cierre de pestaña deja al cliente sin comprobante externo.
- **Evidencia:** no hay servicios de email/notificaciones; `ComplexDetailPage.tsx:352-375` cierra el diálogo después del POST; el dominio lista notificaciones como futuro (`DOMAIN-MODEL.md:249-253`).
- **Recomendación:** para piloto, comprobante visible y compartible/copiable con contacto y condiciones, más una comunicación email transaccional mínima o protocolo explícito de aviso manual. Complejidad **M**; depende de PR-001 y proveedor/decisión PR-021.
- **Aceptación:** toda reserva/cancelación genera comprobante persistente; se registra intento/resultado de comunicación; no se bloquea reserva si un canal asíncrono falla.

### PR-006 — No hay reprogramación de reserva puntual
- **Categoría / prioridad:** Functional Gap, UX/UI Gap — **P1**.
- **Estado:** Inexistente.
- **Escenario:** lluvia, error telefónico o pedido de cliente requiere mover un turno sin perder trazabilidad. El personal hoy debe cancelar y crear otra reserva manualmente.
- **Evidencia:** `ReservationsController.cs:75-232` ofrece crear, cancelar y cambiar a Completed/NoShow; `ComplexReservationsPage.tsx:418-436` sólo expone cancelar/marcar estado.
- **Recomendación:** acción de reprogramar que conserva vínculo con la reserva original, actor, motivo y comprobante al usuario. **M**; depende de PR-003/004/002.
- **Aceptación:** no permite solape, mantiene historial, actualiza precio bajo regla decidida y comunica el cambio.

### PR-007 — Bloqueos de cancha sin operación administrativa
- **Categoría / prioridad:** Functional Gap, Operational Risk — **P1**.
- **Estado:** Parcial: persistencia/consulta implementadas, gestión ausente.
- **Escenario:** mantenimiento o evento debe cerrar horarios antes de que se reserven. La disponibilidad los considera, pero el administrador no puede crearlos desde API/UI.
- **Evidencia:** `CourtBlock` existe y es aplicado en `GetCourtAvailabilityHandler.cs:60-66` y `CreateReservationHandler.cs:85-90`; no existe `CourtBlocksController` y no hay referencias frontend de creación.
- **Recomendación:** CRUD mínimo (crear/listar/cancelar bloqueos futuros) con conflicto explícito frente a reservas existentes. **M**.
- **Aceptación:** bloqueo no aparece como disponible, evita creación de reserva, audita actor/motivo y define qué sucede ante reservas preexistentes.

### PR-008 — Modificación futura de recurrentes no entregada de punta a punta
- **Categoría / prioridad:** Functional Gap, UX/UI Gap — **P1**.
- **Estado:** Parcial.
- **Escenario:** un cliente fijo pasa de lunes a martes desde el mes próximo. El backend puede modificar futuro, pero ningún usuario puede invocarlo desde la aplicación.
- **Evidencia:** `RecurringReservationsController.cs:156-196`; `ModifyRecurringReservationFutureHandler.cs:44-107`; sin coincidencias frontend para `ModifyRecurring/future`.
- **Recomendación:** exponer flujo administrable y decidir si el jugador puede usarlo. **M**. Depende de política de cancelación/reprogramación.
- **Aceptación:** conserva historia, valida conflictos, permite admin autorizado y muestra resultado/errores.

### PR-009 — Activación y administración de complejos depende de SuperAdmin sin consola operativa
- **Categoría / prioridad:** Functional Gap, Operational Risk — **P1**.
- **Estado:** Parcial.
- **Escenario:** un propietario crea complejo `Pending`; alguien debe aprobarlo antes de hacerlo público. La pantalla SuperAdmin sólo enlaza auditoría.
- **Evidencia:** onboarding crea `Pending` (`CompleteComplexAdminHandler.cs:55-71`); hay endpoint de status (`ComplexesController.cs:228-252`) pero el panel es sólo `SuperAdminPage.tsx:5-19`.
- **Recomendación:** para piloto, procedimiento interno autenticado y auditado; para público, lista/approval UI y administración de responsables. **M**.
- **Aceptación:** operador ve pendientes, aprueba/rechaza con motivo, y complejo pendiente no recibe reservas públicas.

### PR-010 — Administradores adicionales no administrables
- **Categoría / prioridad:** Functional Gap — **P2 piloto / P1 público**.
- **Estado:** Existencia no verificable como flujo: asociación de primer admin sí existe, no se halló gestión posterior.
- **Evidencia:** `ComplexAdministrator` está modelado; no hay endpoints/UI de alta/baja de administradores en controllers/rutas.
- **Impacto:** un complejo con más de una persona depende del dueño original. **M**.
- **Aceptación:** invitar/revocar admin de su propio complejo, impedir auto-remoción del último admin y auditar.

### PR-011 — Condiciones de alquiler no se muestran antes de reservar
- **Categoría / prioridad:** Business Gap, UX/UI Gap — **P1**.
- **Estado:** Parcial: sólo existe configuración administrativa de cancelación.
- **Evidencia:** `ConfigurationPage.tsx:127-270`; diálogo público no muestra política (`ComplexDetailPage.tsx:81-119`).
- **Recomendación:** mostrar mínimo de anticipación, precio y contacto antes de confirmar. **S** junto a PR-001.

### PR-012 — Confirmación de reserva no tiene estado de llegada/presentación
- **Categoría / prioridad:** Operational Risk — **P2**.
- **Estado:** Parcial: `Completed` y `NoShow` se pueden marcar.
- **Evidencia:** `Reservation.cs:99-119`, `ComplexReservationsPage.tsx:426-436`.
- **Brecha:** no existe check-in/arribo, ni motivo de no-show ni métricas utilizables para la recepción. Para piloto se puede operar con Completed/NoShow; no es P0.
- **Recomendación:** mantener flujo actual inicialmente, agregar acción rápida/check-in y reporte operativo después del piloto. **M**.

## Flujo jugador evaluado

| Paso | Estado | Observación |
|---|---|---|
| Descubrir y filtrar complejos | Implementado y probado parcialmente | Listado, búsqueda, detalle y deporte; no se evaluó SEO real/analytics. |
| Elegir cancha y ver disponibilidad | Implementado y validado parcialmente | Zonas horarias y bloqueos se consideran; E2E depende de datos externos. |
| Conocer precio/condiciones | Inexistente/parcial | Falta precio, moneda, pago manual y política pública. |
| Reservar | Parcial | Crea reserva confirmada, pero sin idempotencia puntual ni comprobante. |
| Confirmación/cancelación | Parcial | Hay cancelación con política; no hay comunicación ni idempotencia. |
| Reprogramar | Inexistente | Requiere workaround cancelación + nueva reserva. |

## Flujo administrador evaluado

| Capacidad | Estado |
|---|---|
| Onboarding, perfil, canchas, horarios y slots | Implementada y validada parcialmente |
| Agenda/lista, alta manual, cliente, cancelar, Completed/NoShow | Implementada y validada parcialmente |
| Recurrentes | Parcial: crear/listar/cancelar; modificación futura sin UI |
| Bloqueos horarios | Parcial: motor los respeta, no hay gestión |
| Precio/caja/pago manual | Inexistente |
| Reprogramación | Inexistente |
| Gestión de equipo de administradores | Inexistente |
| Aprobación de complejos | Parcial, sin consola de operación |

## Funcionalidades deliberadamente diferibles

- Tarifas por día/franja/feriado, descuentos de recurrentes, promociones: **P2** tras tarifa base e histórico.
- Pago online, depósitos, facturación, conciliación bancaria: **P3**, fuera del MVP.
- WhatsApp/SMS y push: **P2**; email o comprobante mínimo sí es necesario para piloto.
- Analítica avanzada, CRM, torneos, marketplace: **P3**.
