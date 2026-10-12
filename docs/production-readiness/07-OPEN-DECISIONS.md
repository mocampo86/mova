# Mova — Decisiones abiertas

| ID | Decisión requerida | Alternativas y recomendación | Impacto / dueño sugerido |
|---|---|---|---|
| PR-020 | Modelo comercial inicial | **Recomendado:** tarifa fija por cancha/slot, moneda por complejo, total snapshot y pago manual. Alternativa: tarifa por duración proporcional. No iniciar con precios por franja/feriado. | Bloquea PR-001/006; Product + Architect. |
| PR-021 | Confirmación/comunicación piloto | Email transaccional mínimo; o comprobante web compartible + proceso manual de contacto acordado. No depender sólo de memoria del usuario. | Bloquea PR-002; Product + Ops. |
| PR-022 | Política de reprogramación | ¿Quién puede reprogramar? ¿Hasta cuándo? ¿Se mantiene tarifa original o se recalcula? ¿Debe confirmar jugador? | Bloquea PR-006; Product. |
| PR-023 | Bloque versus reserva existente | Prohibir bloque conflictivo, pedir cancelación/reprogramación previa, o permitirlo y notificar. Recomendado: prohibir por defecto y ofrecer flujo explícito. | PR-007; Product + Architect. |
| PR-024 | Actor de modificación de serie | Admin solamente vs. admin y propietario. Recomendado: admin y propietario con reglas de deadline; admin puede actuar para cliente. | PR-008; Product + Security. |
| PR-025 | Activación de complejos | Aprobación manual central en piloto; autoactivación con verificación para público. Definir SLA/motivos de rechazo. | PR-009; Product + Operations. |
| PR-026 | Gestión de administradores | ¿Invitación por email? Roles Manager/Admin? ¿quién puede revocar? Recomendado: un rol Admin y protección del último activo inicialmente. | PR-010; Product + Security. |
| PR-027 | Estrategia de integridad DB | Serializable + retry/mapeo 409 solamente, o agregar exclusion constraint PostgreSQL para activos. Recomendado: mantener serializable y añadir defensa declarativa evaluada/probada. | PR-004; Architect + Backend. |
| PR-028 | Idempotencia | Ámbito, TTL, qué respuestas se persisten y cómo se coordinan requests concurrentes. Recomendado: actor+operación+clave, 24h configurable, almacenar éxito final; manejar in-progress explícitamente. | PR-003/013; Architect + Backend. |
| PR-029 | Sesión y cuenta bloqueada global | Mantener JWT 15min sin refresh para piloto o introducir refresh HttpOnly/revocación para público. Definir proceso de soporte ante cuenta bloqueada/autenticación Google fallida. | PR-017; Security + Product. |
| PR-030 | Despliegue Azure | App Service vs Container Apps, región, RPO/RTO, responsables de alertas y presupuesto. | PR-015/016; DevOps + sponsor. |
| PR-031 | Datos y cumplimiento | Política de privacidad, retención/borrado, uso de teléfono, logs/audit y términos de cancelación. | Público; Product + Legal/Security. |

## Decisiones ya razonables para conservar

- Monolito modular y PostgreSQL compartido con aislamiento lógico para MVP.
- Google OIDC + JWT de corta vida, siempre que secretos/configuración productiva se administren correctamente.
- Sin pagos online para MVP; esto **no** elimina necesidad de precio, moneda, condiciones ni pago manual.
- Recurrencia semanal acotada y horarios locales con IANA.
