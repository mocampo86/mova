# Reserva Canchas — Documento General del Proyecto

## 1. Información general

**Nombre provisional:** Reserva Canchas  
**Tipo de producto:** Plataforma web para gestión y reserva de canchas deportivas  
**Estado:** Definición inicial del MVP  
**Plataformas:** Web responsive para PC, tablet y dispositivos móviles  

## 2. Visión del producto

Reserva Canchas será una plataforma web que permitirá a complejos deportivos, clubes y propietarios publicar y administrar sus canchas, mientras que los usuarios podrán consultar disponibilidad y realizar reservas de forma simple.

La solución estará preparada para administrar canchas de fútbol y de cualquier otro deporte, evitando que el modelo de negocio quede limitado a una única disciplina.

El producto contará con:

- Una web pública y visualmente atractiva.
- Un portal para usuarios.
- Un panel de administración para los responsables de los complejos.
- Una API desarrollada en .NET.
- Una aplicación web desarrollada en React.
- Una base de datos PostgreSQL.

En la primera versión no se procesarán pagos en línea.

---

## 3. Objetivo del MVP

El MVP debe permitir validar que usuarios y complejos deportivos pueden gestionar reservas sin depender de llamadas telefónicas, mensajes o agendas manuales.

El sistema deberá cubrir como mínimo el siguiente flujo:

1. Un administrador registra un complejo deportivo.
2. El administrador crea una o más canchas.
3. Configura deportes, días, horarios y duración de los turnos.
4. Un usuario se registra mediante Google.
5. El usuario completa sus datos básicos y número de teléfono.
6. Consulta la disponibilidad de una cancha.
7. Realiza una reserva puntual.
8. El administrador visualiza y administra la reserva.
9. El administrador puede bloquear horarios o usuarios.
10. Se pueden administrar reservas fijas semanales.

---

## 4. Alcance funcional del MVP

### 4.1 Funcionalidades incluidas

- Landing page pública.
- Diseño responsive para PC y mobile.
- Registro e inicio de sesión mediante Google.
- Perfil de usuario.
- Número de teléfono obligatorio.
- Administración de complejos deportivos.
- Administración de canchas.
- Asociación de uno o varios deportes a una cancha.
- Configuración de días y horarios de funcionamiento.
- Configuración de duración de los turnos.
- Consulta de disponibilidad por fecha y cancha.
- Reserva puntual de una cancha.
- Reserva recurrente con frecuencia semanal.
- Consulta de reservas del usuario.
- Consulta administrativa de reservas.
- Cancelación de reservas.
- Creación manual de reservas por parte de un administrador.
- Bloqueo de horarios.
- Bloqueo y desbloqueo de usuarios.
- Historial básico de reservas.
- Roles y autorización.
- Registro básico de auditoría.
- Manejo global de errores.
- Logging estructurado.
- Health checks.
- Documentación de la API mediante OpenAPI/Swagger.

### 4.2 Fuera del alcance del MVP

- Pagos en línea.
- Señas o depósitos mediante la plataforma.
- Validación del teléfono mediante SMS o WhatsApp.
- Aplicaciones móviles nativas.
- Notificaciones push.
- Chat entre usuarios y administradores.
- Cupones o promociones.
- Precios dinámicos.
- Torneos, campeonatos o ligas.
- Sistema de reputación o comentarios.
- Integración contable.
- Facturación electrónica.
- Marketplace con cobro de comisiones.
- Integración con cerraduras o controles de acceso.

---

## 5. Roles del sistema

### 5.1 Usuario

El usuario final podrá:

- Registrarse con Google.
- Completar nombre, correo y teléfono.
- Consultar complejos deportivos.
- Consultar canchas y deportes disponibles.
- Ver horarios disponibles.
- Reservar una cancha.
- Consultar sus próximas reservas.
- Consultar su historial.
- Cancelar una reserva según las reglas configuradas.
- Visualizar si su cuenta está bloqueada en un complejo.

### 5.2 Administrador de complejo

El administrador podrá:

- Crear y editar la información de su complejo.
- Crear, editar, activar o desactivar canchas.
- Asociar deportes a las canchas.
- Configurar días y horarios de funcionamiento.
- Configurar la duración de los turnos.
- Consultar las reservas del complejo.
- Crear reservas manualmente.
- Cancelar reservas.
- Crear reservas recurrentes semanales.
- Bloquear horarios por mantenimiento, eventos u otros motivos.
- Bloquear y desbloquear usuarios.
- Consultar información básica e historial de los clientes.
- Administrar otros responsables del complejo, en una etapa posterior del MVP si fuera necesario.

### 5.3 Superadministrador

El rol debe existir en la arquitectura, aunque su panel completo puede desarrollarse después del MVP.

Podrá:

- Consultar todos los complejos.
- Activar o desactivar complejos.
- Administrar responsables.
- Resolver incidencias operativas.
- Consultar métricas globales.
- Acceder a auditoría global.

---

## 6. Reglas de negocio principales

### 6.1 Reservas puntuales

Una reserva puntual corresponde a una cancha, un usuario y un rango horario determinado.

La duración inicial sugerida es de una hora, pero el modelo debe permitir que cada cancha configure la duración de sus turnos.

Una reserva debe incluir:

- Complejo.
- Cancha.
- Usuario.
- Fecha y hora de inicio.
- Fecha y hora de finalización.
- Estado.
- Origen de la reserva.
- Fecha de creación.
- Observaciones opcionales.

Estados sugeridos:

- `Pending`
- `Confirmed`
- `CancelledByUser`
- `CancelledByAdmin`
- `Completed`
- `NoShow`

Mientras no existan pagos, una reserva realizada por un usuario puede quedar confirmada inmediatamente.

### 6.2 Prevención de conflictos

No pueden existir dos reservas activas superpuestas para una misma cancha.

La validación deberá realizarse:

- En la capa de aplicación.
- Dentro de una transacción.
- Con una estrategia de persistencia que evite conflictos ante solicitudes concurrentes.

Este comportamiento deberá contar con pruebas de integración y concurrencia.

### 6.3 Reservas recurrentes

Una reserva fija semanal deberá modelarse mediante:

- Una entidad que represente la recurrencia.
- Una regla semanal.
- Reservas individuales generadas para cada ocurrencia.

Ejemplo:

> Todos los miércoles de 20:00 a 21:00, desde el 1 de agosto hasta el 31 de diciembre.

Esto permitirá:

- Cancelar una ocurrencia específica.
- Cancelar toda la serie.
- Detectar conflictos en fechas concretas.
- Modificar futuras ocurrencias sin alterar el historial.

Para el MVP, toda recurrencia deberá tener una fecha de finalización o un máximo configurable de semanas.

### 6.4 Bloqueo de usuarios

El bloqueo deberá aplicarse por complejo deportivo.

Un usuario bloqueado en un complejo podrá continuar utilizando otros complejos, salvo que también esté bloqueado en ellos.

El bloqueo debe registrar:

- Usuario.
- Complejo.
- Motivo.
- Fecha de bloqueo.
- Administrador responsable.
- Fecha de vencimiento opcional.
- Estado.

Un usuario bloqueado no podrá crear nuevas reservas en ese complejo.

### 6.5 Bloqueo de horarios

Un administrador podrá bloquear una cancha durante un rango horario por:

- Mantenimiento.
- Eventos.
- Uso interno.
- Feriados.
- Cierre excepcional.
- Otros motivos.

Un horario bloqueado no deberá mostrarse como disponible.

### 6.6 Cancelaciones

La política de cancelación debe quedar preparada para ser configurable por complejo.

Para el MVP se podrá utilizar una regla global o una cantidad mínima de horas antes del turno.

Toda cancelación debe registrar:

- Usuario o administrador que la realizó.
- Fecha y hora.
- Motivo opcional.
- Estado anterior y nuevo estado.

### 6.7 Teléfono

El número de teléfono será obligatorio para completar el registro.

En el MVP:

- Se validará su formato.
- No se verificará mediante SMS o WhatsApp.
- Se mantendrá un indicador `PhoneVerified` preparado para una implementación futura.

---

## 7. Experiencia web

La aplicación deberá diseñarse con enfoque **mobile first**.

### 7.1 Mobile

- Menú hamburguesa o navegación inferior.
- Formularios en una sola columna.
- Botones grandes y accesibles.
- Selección de horarios mediante tarjetas o botones.
- Calendario simplificado.
- Evitar tablas horizontales difíciles de utilizar.
- Acciones administrativas adaptadas a pantallas pequeñas.

### 7.2 Desktop

- Menú lateral para el panel administrativo.
- Calendario diario o semanal.
- Tablas con filtros y paginación.
- Formularios en paneles, páginas o modales.
- Visualización simultánea de canchas y horarios.

### 7.3 Aplicación única

Para el MVP se recomienda una sola aplicación React con:

- Layout público.
- Layout de usuario.
- Layout administrativo.
- Rutas y componentes protegidos por permisos.

No se recomienda separar inicialmente el panel administrativo y el portal del usuario en aplicaciones diferentes.

---

## 8. Stack tecnológico

### 8.1 Backend

- .NET 10, o .NET 8 LTS si se prioriza una plataforma más conservadora.
- ASP.NET Core Web API.
- Entity Framework Core.
- Npgsql.
- PostgreSQL.
- FluentValidation.
- Serilog.
- OpenAPI / Swagger.
- Google OpenID Connect.
- JWT Bearer Authentication.
- xUnit.
- FluentAssertions.
- Testcontainers para pruebas con PostgreSQL.

### 8.2 Frontend

- React.
- TypeScript.
- Vite.
- React Router.
- TanStack Query.
- React Hook Form.
- Zod.
- Material UI.
- Vitest.
- React Testing Library.
- Playwright para pruebas end-to-end críticas.

### 8.3 Base de datos

- PostgreSQL.
- Una única base de datos compartida para el MVP.
- Aislamiento lógico mediante `SportsComplexId`.
- Migraciones administradas con Entity Framework Core.

### 8.4 Infraestructura local

- Docker Compose.
- PostgreSQL en contenedor.
- API y frontend ejecutables localmente.
- Variables de entorno mediante archivos locales no versionados.

### 8.5 Infraestructura sugerida para despliegue

- API: Azure App Service o Azure Container Apps.
- Frontend: Azure Static Web Apps.
- Base de datos: Azure Database for PostgreSQL.
- Secretos: Azure Key Vault.
- Observabilidad: Application Insights.
- CI/CD: GitHub Actions o Azure DevOps Pipelines.

---

## 9. Arquitectura de software

Se recomienda implementar un **monolito modular**.

Esta arquitectura permite:

- Mantener bajo el costo inicial.
- Reducir complejidad operativa.
- Separar correctamente responsabilidades.
- Evolucionar módulos de forma independiente.
- Extraer servicios en el futuro si fuera necesario.

### 9.1 Estructura del backend

```text
src/
├── ReservaCanchas.Api
├── ReservaCanchas.Application
├── ReservaCanchas.Domain
├── ReservaCanchas.Infrastructure
└── ReservaCanchas.Contracts

tests/
├── ReservaCanchas.UnitTests
├── ReservaCanchas.IntegrationTests
└── ReservaCanchas.ArchitectureTests
```

### 9.2 Responsabilidades

#### ReservaCanchas.Api

- Endpoints HTTP.
- Configuración de la aplicación.
- Autenticación y autorización.
- Middlewares.
- OpenAPI.
- Health checks.

#### ReservaCanchas.Application

- Casos de uso.
- Commands y queries.
- Validaciones.
- Interfaces de servicios externos.
- Orquestación de reglas de negocio.

#### ReservaCanchas.Domain

- Entidades.
- Value Objects.
- Reglas de negocio.
- Eventos de dominio, si resultan necesarios.
- Enumeraciones y contratos propios del dominio.

#### ReservaCanchas.Infrastructure

- Entity Framework Core.
- PostgreSQL.
- Repositorios.
- Servicios de autenticación.
- Integraciones externas.
- Persistencia de auditoría.

#### ReservaCanchas.Contracts

- Requests.
- Responses.
- Contratos públicos de la API.
- Modelos de paginación y errores.

### 9.3 Estructura del frontend

```text
src/
├── app/
├── components/
├── layouts/
├── pages/
├── features/
│   ├── auth/
│   ├── users/
│   ├── complexes/
│   ├── courts/
│   ├── availability/
│   ├── reservations/
│   ├── recurring-reservations/
│   └── administration/
├── services/
├── hooks/
└── shared/
```

---

## 10. Estrategia multi-complejo

El sistema debe soportar múltiples complejos desde el inicio.

Para el MVP se utilizará:

- Una base PostgreSQL compartida.
- Un identificador `SportsComplexId` en las entidades correspondientes.
- Autorización para garantizar que un administrador acceda únicamente a sus complejos.
- Filtros obligatorios por complejo en consultas administrativas.

No se utilizará una base de datos por complejo durante el MVP.

Una base por cliente agregaría complejidad innecesaria en:

- Aprovisionamiento.
- Migraciones.
- Backups.
- Monitoreo.
- Conexiones dinámicas.
- Costos de infraestructura.

---

## 11. Modelo de datos inicial

### 11.1 User

- `Id`
- `GoogleSubjectId`
- `Email`
- `FullName`
- `PhoneNumber`
- `PhoneVerified`
- `Status`
- `CreatedAt`
- `UpdatedAt`

### 11.2 SportsComplex

- `Id`
- `Name`
- `Description`
- `Address`
- `City`
- `Latitude`
- `Longitude`
- `PhoneNumber`
- `Email`
- `Status`
- `CreatedAt`
- `UpdatedAt`

### 11.3 ComplexAdministrator

- `Id`
- `SportsComplexId`
- `UserId`
- `Role`
- `Status`
- `CreatedAt`

### 11.4 Sport

- `Id`
- `Name`
- `Status`

### 11.5 Court

- `Id`
- `SportsComplexId`
- `Name`
- `Description`
- `SurfaceType`
- `Indoor`
- `Status`
- `CreatedAt`
- `UpdatedAt`

### 11.6 CourtSport

- `CourtId`
- `SportId`

### 11.7 BusinessHours

- `Id`
- `SportsComplexId`
- `DayOfWeek`
- `OpeningTime`
- `ClosingTime`
- `IsClosed`

### 11.8 CourtAvailabilityRule

- `Id`
- `CourtId`
- `DayOfWeek`
- `StartTime`
- `EndTime`
- `SlotDurationMinutes`
- `IsActive`

### 11.9 Reservation

- `Id`
- `SportsComplexId`
- `CourtId`
- `UserId`
- `StartAt`
- `EndAt`
- `Status`
- `Source`
- `RecurringReservationId`
- `Notes`
- `CreatedAt`
- `CancelledAt`
- `CancellationReason`

### 11.10 RecurringReservation

- `Id`
- `SportsComplexId`
- `CourtId`
- `UserId`
- `DayOfWeek`
- `StartTime`
- `DurationMinutes`
- `StartDate`
- `EndDate`
- `Status`
- `CreatedAt`

### 11.11 CourtBlock

- `Id`
- `SportsComplexId`
- `CourtId`
- `StartAt`
- `EndAt`
- `Reason`
- `CreatedByUserId`
- `CreatedAt`

### 11.12 BlockedUser

- `Id`
- `SportsComplexId`
- `UserId`
- `Reason`
- `BlockedAt`
- `BlockedUntil`
- `BlockedByUserId`
- `Status`

### 11.13 AuditLog

- `Id`
- `UserId`
- `SportsComplexId`
- `Action`
- `EntityType`
- `EntityId`
- `CreatedAt`
- `Metadata`

---

## 12. Seguridad

- Autenticación mediante Google OpenID Connect.
- Emisión o validación de JWT para acceder a la API.
- Autorización basada en roles y permisos.
- Validación de pertenencia del administrador al complejo.
- Secretos fuera del repositorio.
- HTTPS obligatorio fuera del entorno local.
- No registrar tokens, teléfonos completos ni información sensible en logs.
- Validación de todos los datos de entrada.
- Protección contra acceso horizontal entre complejos.
- Auditoría de acciones administrativas relevantes.
- Rate limiting preparado para endpoints sensibles.

Roles iniciales sugeridos:

- `User`
- `ComplexAdmin`
- `SuperAdmin`

---

## 13. Contratos y convenciones de API

### 13.1 Convenciones

- Endpoints versionados bajo `/api/v1`.
- Recursos expresados en plural.
- Fechas y horas en UTC en backend y base de datos.
- Conversión a zona horaria local en frontend.
- Paginación para listados administrativos.
- Filtros mediante query string.
- Códigos HTTP consistentes.

### 13.2 Contrato de error sugerido

```json
{
  "error": {
    "code": "RESERVATION_CONFLICT",
    "message": "The selected time is no longer available.",
    "details": {},
    "traceId": "00-..."
  }
}
```

### 13.3 Respuestas paginadas

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalItems": 0,
  "totalPages": 0
}
```

---

## 14. Repositorios

Se recomiendan dos repositorios:

```text
reserva-canchas-api
reserva-canchas-web
```

### 14.1 reserva-canchas-api

Contendrá:

- API .NET.
- Dominio.
- Aplicación.
- Infraestructura.
- Migraciones.
- Tests.
- Dockerfile.
- Documentación técnica de backend.

### 14.2 reserva-canchas-web

Contendrá:

- React.
- Landing page.
- Portal de usuario.
- Panel administrativo.
- Tests.
- Configuración de despliegue.

---

## 15. Backlog de alto nivel

### Epic 01 — Fundación técnica

- Configuración inicial de la solución backend.
- Configuración inicial del frontend.
- PostgreSQL y Entity Framework Core.
- Configuración por ambientes.
- Manejo global de errores.
- Logging estructurado.
- Docker y entorno local.
- Pipeline de integración continua.
- Health checks.

### Epic 02 — Identidad y acceso

- Login con Google.
- Registro del usuario.
- Perfil de usuario.
- Número de teléfono obligatorio.
- Roles y autorización.
- Protección de rutas y endpoints.
- Preparación para verificación futura del teléfono.

### Epic 03 — Administración de complejos

- Crear complejo.
- Editar complejo.
- Consultar complejo.
- Activar o desactivar complejo.
- Administrar responsables.
- Configurar información pública.
- Configurar dirección y ubicación.

### Epic 04 — Administración de canchas

- Crear cancha.
- Editar cancha.
- Consultar cancha.
- Activar o desactivar cancha.
- Asociar deportes.
- Configurar horarios.
- Configurar duración de turnos.
- Bloquear horarios.

### Epic 05 — Consulta pública

- Listar complejos.
- Consultar detalle de un complejo.
- Listar canchas.
- Filtrar por deporte.
- Consultar disponibilidad por fecha.
- Visualizar próximos horarios disponibles.

### Epic 06 — Reservas

- Crear reserva.
- Consultar reserva.
- Listar reservas del usuario.
- Listar reservas administrativas.
- Cancelar reserva.
- Crear reserva manual.
- Evitar reservas superpuestas.
- Registrar inasistencia.
- Marcar reserva como completada.

### Epic 07 — Reservas recurrentes

- Crear reserva semanal.
- Validar disponibilidad del período.
- Generar reservas individuales.
- Cancelar una ocurrencia.
- Cancelar toda la recurrencia.
- Modificar una recurrencia.
- Detectar conflictos futuros.

### Epic 08 — Gestión de usuarios

- Buscar usuarios del complejo.
- Consultar historial de reservas.
- Bloquear usuario.
- Desbloquear usuario.
- Definir bloqueo temporal.
- Impedir reservas de usuarios bloqueados.
- Registrar motivo del bloqueo.

### Epic 09 — Landing page

- Página de inicio.
- Explicación del funcionamiento.
- Sección para jugadores.
- Sección para propietarios.
- Complejos destacados.
- Acceso de usuario.
- Acceso administrativo.
- Diseño responsive.
- SEO básico.

### Epic 10 — Auditoría y operación

- Auditoría de acciones administrativas.
- Monitoreo de errores.
- Health checks.
- Métricas básicas.
- Documentación operativa.
- Backup y recuperación.

---

## 16. Plan de entregas

### Entrega 1 — Base técnica

- Solución backend.
- Frontend inicial.
- PostgreSQL.
- Docker Compose.
- Logging.
- Manejo de errores.
- CI.
- Documentación de desarrollo local.

### Entrega 2 — Autenticación

- Google Login.
- Registro.
- Perfil.
- Teléfono obligatorio.
- Roles.
- Protección de rutas y endpoints.

### Entrega 3 — Complejos y canchas

- CRUD de complejos.
- CRUD de canchas.
- Deportes.
- Horarios de funcionamiento.
- Panel administrativo inicial.

### Entrega 4 — Disponibilidad y reservas

- Calendario.
- Consulta de disponibilidad.
- Creación de reservas.
- Cancelación.
- Prevención de conflictos.
- Historial del usuario.

### Entrega 5 — Administración avanzada

- Reservas manuales.
- Bloqueos de horarios.
- Bloqueo de usuarios.
- Historial administrativo.
- Auditoría.

### Entrega 6 — Reservas semanales

- Recurrencias.
- Generación de ocurrencias.
- Conflictos.
- Cancelaciones parciales y completas.

### Entrega 7 — Landing y despliegue

- Landing visual.
- Diseño responsive final.
- SEO básico.
- Despliegue.
- Monitoreo.
- Documentación operativa.

---

## 17. Estrategia de pruebas

### Backend

- Pruebas unitarias para reglas de negocio.
- Pruebas de integración para endpoints y persistencia.
- PostgreSQL real mediante Testcontainers.
- Pruebas de concurrencia para creación de reservas.
- Pruebas de autorización por complejo.
- Pruebas de migraciones.

### Frontend

- Pruebas unitarias para utilidades y componentes críticos.
- React Testing Library para formularios y flujos.
- Playwright para:
  - Login.
  - Consulta de disponibilidad.
  - Creación de reserva.
  - Cancelación.
  - Creación de cancha por administrador.

### Criterio general

Todo caso de uso crítico deberá contar con pruebas automatizadas antes de considerarse terminado.

---

## 18. Lineamientos para Devin

Cada tarea entregada a Devin deberá incluir:

- Objetivo concreto.
- Contexto funcional.
- Proyecto y archivos afectados.
- Acceptance Criteria verificables.
- Reglas de negocio.
- Casos de error.
- Consideraciones de seguridad.
- Pruebas requeridas.
- Out of Scope.
- Dependencias.
- Comandos de validación.
- Definition of Done.

### Formato sugerido para User Stories

```markdown
# US-XXX — Nombre

## Objetivo

## Descripción funcional

## Acceptance Criteria

## Reglas de negocio

## Validaciones

## Casos de error

## Consideraciones de seguridad

## Consideraciones técnicas

## Pruebas requeridas

## Out of Scope

## Dependencias

## Definition of Done

## Proyecto afectado

## Rama sugerida

## Instrucciones para Devin
```

### Reglas de implementación

- No modificar funcionalidades fuera del alcance de la tarea.
- No incorporar dependencias sin justificar su necesidad.
- Mantener separación entre dominio, aplicación, infraestructura y API.
- No exponer entidades de persistencia directamente desde los endpoints.
- No almacenar secretos en el repositorio.
- Agregar migraciones cuando cambie el modelo de datos.
- Agregar o actualizar pruebas.
- Ejecutar build, tests y análisis estático antes de finalizar.
- Documentar decisiones relevantes.
- Informar supuestos y limitaciones en el Pull Request.

---

## 19. Definition of Done general

Una historia se considera terminada cuando:

- Cumple todos los Acceptance Criteria.
- El proyecto compila sin errores.
- Los tests existentes continúan pasando.
- Se agregaron las pruebas necesarias.
- No se introdujeron vulnerabilidades evidentes.
- No se exponen secretos ni datos sensibles.
- Las migraciones fueron creadas y validadas cuando correspondía.
- La documentación fue actualizada.
- La funcionalidad fue validada manualmente.
- El código cumple las convenciones del proyecto.
- La solución funciona tanto en desktop como en mobile cuando afecta al frontend.

---

## 20. Decisiones pendientes

Las siguientes decisiones deberán resolverse durante el refinamiento:

- Nombre definitivo del producto.
- Uso de .NET 10 o .NET 8 LTS.
- Proveedor definitivo de hosting.
- Política inicial de cancelación.
- Duración configurable por cancha o por complejo.
- Cantidad máxima de semanas para reservas recurrentes.
- Posibilidad de que un administrador gestione varios complejos.
- Flujo para registrar el primer administrador de un complejo.
- Moderación o aprobación de nuevos complejos.
- Zona horaria por complejo.
- Gestión de precios, aunque no existan pagos en línea.
- Envío de correos de confirmación dentro o después del MVP.
- Política de eliminación y conservación de datos.

---

## 21. Próximos documentos recomendados

A partir de este documento general, se recomienda crear:

```text
docs/
├── product/
│   ├── PRODUCT-VISION.md
│   ├── MVP-SCOPE.md
│   ├── ROLES-AND-PERMISSIONS.md
│   ├── BUSINESS-RULES.md
│   └── GLOSSARY.md
├── architecture/
│   ├── SOLUTION-ARCHITECTURE.md
│   ├── DOMAIN-MODEL.md
│   ├── DATABASE-DESIGN.md
│   ├── AUTHENTICATION.md
│   ├── MULTI-TENANCY.md
│   └── DEPLOYMENT.md
├── development/
│   ├── DEVELOPMENT-GUIDELINES.md
│   ├── TESTING-STRATEGY.md
│   ├── GIT-STRATEGY.md
│   ├── DEFINITION-OF-DONE.md
│   └── DEVIN-INSTRUCTIONS.md
├── backlog/
│   ├── EPIC-01-TECHNICAL-FOUNDATION.md
│   ├── EPIC-02-IDENTITY.md
│   ├── EPIC-03-COMPLEXES.md
│   ├── EPIC-04-COURTS.md
│   ├── EPIC-05-AVAILABILITY.md
│   ├── EPIC-06-RESERVATIONS.md
│   ├── EPIC-07-RECURRING-RESERVATIONS.md
│   ├── EPIC-08-USERS.md
│   ├── EPIC-09-LANDING.md
│   └── EPIC-10-OPERATIONS.md
└── api/
    ├── API-CONVENTIONS.md
    ├── ERROR-CONTRACT.md
    ├── PAGINATION.md
    └── IDEMPOTENCY.md
```

---

## 22. Resumen de decisiones confirmadas

| Área | Decisión |
|---|---|
| Backend | ASP.NET Core Web API en .NET |
| Frontend | React con TypeScript |
| Diseño | Responsive y mobile first |
| Base de datos | PostgreSQL |
| Arquitectura | Monolito modular |
| Multi-complejo | Base compartida con aislamiento lógico |
| Autenticación | Google OpenID Connect y JWT |
| Pagos en línea | Fuera del MVP |
| Verificación telefónica | Preparada, pero fuera del MVP |
| Aplicaciones web | Una aplicación React con layouts por rol |
| Repositorios | API y web separados |
| Entorno local | Docker Compose |

