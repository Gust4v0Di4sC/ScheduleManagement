# ScheduleManagement

ASP.NET Core .NET 10 API for a multi-tenant service-booking platform. The MVP is being built as a modular monolith with one executable host.

## Current state

- The API builds and runs on .NET 10.
- Liveness is exposed at `/health/live`.
- OpenAPI is exposed at `/openapi/v1.json` in Development.
- Platform middleware provides correlation IDs, security headers, Problem Details and basic rate limiting.
- Business modules currently share the host assembly and are separated by folders and namespaces.
- Domain and application foundations cover account onboarding, account-scoped identity, catalog, professionals, availability, quotation, appointment transitions, rescheduling, payment, notifications, audit, outbox and local exports.
- Development adapters include a captured-email mailbox, local object storage, a simulated idempotent payment gateway and expiring account exports.
- PostgreSQL persistence and authentication are not implemented yet; their dependencies are recorded in the MVP pending-decisions document.

## Documentation

- [Architecture](docs/architecture.md)
- [Local setup](docs/setup.md)
- [PostgreSQL and migrations](docs/postgresql.md)
- [Validation](docs/validation.md)
- [MVP pending decisions](docs/mvp-pending-decisions.md)
- [MVP implementation progress](docs/mvp-progress.md)
