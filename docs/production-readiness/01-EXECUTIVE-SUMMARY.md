# Mova — Executive Summary de preparación para producción

**Fecha de auditoría:** 2026-10-12  
**Alcance:** repositorio, documentación, pruebas y configuraciones versionadas. No se modificó código ni infraestructura.

## Dictamen

**Mova no está lista para un lanzamiento público ni para operar comercialmente tal como está.** Puede llegar a un **piloto controlado** después de resolver los P0/P1 de este informe, especialmente precio/condiciones, confirmación operativa, defensa de integridad de reservas y una validación de calidad reproducible.

El producto ya tiene una base sólida: arquitectura modular, disponibilidad con zonas horarias, reservas únicas y recurrentes, calendario administrativo, bloqueos, cancelación configurable, aislamiento administrativo probado en parte, logging, health checks, rate limiting y documentación operativa. Sin embargo, el flujo comercial central termina en una reserva sin precio, moneda, estado de pago ni confirmación persistente/comunicada.

## Estimación de preparación

### Resultado: **aprox. 58% listo para piloto** y **40% listo para lanzamiento público**

Método: se evaluaron 43 capacidades ponderadas en seis dominios: flujo jugador (20%), operación/comercial (25%), integridad y seguridad (20%), UX (10%), calidad (15%) e infraestructura/operación (10%). Cada capacidad recibió 0 (ausente), 0,5 (parcial/no probada) o 1 (implementada y evidenciada); la cifra mide **capacidad de producto**, no cobertura de líneas ni esfuerzo ya invertido.

Limitaciones: no hubo acceso a Azure, dominio/Key Vault, datos productivos ni dispositivos físicos; no se ejecutó un E2E contra API/DB real. La suite .NET no quedó completamente verde. Por ello los porcentajes son conservadores y no son una certificación de seguridad ni de cumplimiento.

## Principales bloqueantes

| ID | Prioridad | Bloqueante |
|---|---:|---|
| PR-001 | P0 | No existe modelo, API ni UI de precio, moneda, total acordado o estado de pago manual. |
| PR-002 | P0 | No hay comunicaciones/confirmación de reserva ni instrucciones comerciales; una reserva confirmada sólo queda en la interfaz. |
| PR-003 | P0 | La reserva puntual y sus cancelaciones/mutaciones no requieren idempotencia, aunque la arquitectura la exige. |
| PR-004 | P0 | La exclusión de solapamientos depende de transacción/consulta; no hay constraint PostgreSQL y no hay prueba concurrente real. |
| PR-005 | P0 | El resultado de calidad no es liberable: `dotnet test` terminó 154 correctas y 4 incorrectas. |
| PR-006 | P1 | No existe reprogramación de reserva puntual; el administrador sólo puede cancelar y recrear. |
| PR-007 | P1 | Bloqueos de cancha se consultan para disponibilidad, pero no existe endpoint/UI para crearlos o gestionarlos. |
| PR-008 | P1 | La modificación futura de series existe en backend, pero no se expone en la SPA. |

## Modelo comercial mínimo recomendado

No hace falta pagos online para el piloto. Sí hace falta que toda reserva tenga una propuesta comercial inequívoca:

1. **Moneda por complejo** (por ejemplo, ISO-4217).
2. **Tarifa base vigente por cancha y duración de slot**; inicialmente una tarifa única por cancha es suficiente.
3. **Total calculado y snapshot inmutable en la reserva** (`unitPrice`, `totalPrice`, `currency`, `pricingVersion/description`). Cambiar tarifas sólo impacta reservas nuevas.
4. **Estado de cobro manual:** `Unpaid`, `Paid`, `PartiallyPaid`, `Waived`/`NotRequired`, más importe recibido, método/referencia opcional y actor/fecha de actualización.
5. Mostrar precio, política de cancelación y datos de contacto **antes de confirmar** y en el comprobante.

Precios por día/franja, feriados, descuentos por recurrencia e impuestos desglosados son P2/P3; no deben bloquear el piloto si la tarifa simple y el snapshot funcionan.

## Recomendación de ruta corta

1. Resolver PR-001 a PR-005 y ejecutar una prueba de humo real con una base PostgreSQL aislada.
2. Completar operación cotidiana: bloqueos, reprogramación explícita, registro de pago manual y confirmación comunicable.
3. Pilotear con 1–3 complejos, soporte cercano y un protocolo temporal documentado para cobro y comunicaciones mientras se mide el uso.
4. Antes de apertura pública, automatizar despliegue/migración controlada, E2E de flujo crítico real, restore drill y configuración Azure verificada.

Véanse: [análisis funcional](02-FUNCTIONAL-GAP-ANALYSIS.md), [diagnóstico técnico](03-TECHNICAL-READINESS.md), [backlog](04-PRIORITIZED-BACKLOG.md) y [roadmap](05-LAUNCH-ROADMAP.md).
