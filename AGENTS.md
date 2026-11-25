# ScheduleManagement agent context

## Architecture

- Keep `Reservas.Api` as the single executable and deployment unit.
- Organize business capabilities under `src/Reservas.Api/Modules/<Module>`.
- Start with module folders in the host assembly. Add a project only when a concrete compile-time boundary is required and document that reason in the pull request.
- A module must not query another module's persistence directly. Cross-module collaboration uses application contracts or events.
- Domain code must not depend on ASP.NET Core, EF Core, storage providers, messaging providers, or payment providers.
- Put HTTP parsing, input validation, authorization, and response mapping in `Presentation`; orchestration in `Application`; invariants in `Domain`; persistence and adapters in `Infrastructure`.

## Multi-tenancy and persistence

- Derive `account_id` from the authenticated context. Never trust an arbitrary account identifier supplied by a tenant client.
- Identity is scoped by account. Coordinate mappings and migrations with Linear issues WOR-112, WOR-114, and WOR-116.
- PostgreSQL mappings and migrations belong to the owning module, use domain schemas and `lower_snake_case`, and require the corresponding database contract to be agreed first.
- Do not create an in-memory or temporary schema and report it as PostgreSQL acceptance.
- RLS is pending. Application filters, compound constraints, and isolation tests remain mandatory.

## API and integrations

- Preserve `/health/live` and the development OpenAPI document.
- Use Problem Details for errors and never expose stack traces or secrets.
- The public-site route currently comes from WOR-39 as `/publico/{site}`. Do not change or stabilize it until it is aligned with WOR-94.
- Development integrations use replaceable local adapters, including a captured-email mailbox for invitations and password recovery.

## Quality checks

- Run `dotnet build Reservas.slnx --no-restore` and `dotnet test Reservas.slnx --no-build` after each batch.
- Use MSTest, FluentAssertions, and Moq. Follow Arrange/Act/Assert and cover success and failure paths.
- Add XML documentation to externally visible public APIs. Prefer internal types for implementation details.
- Test domain rules without HTTP or EF Core. Use PostgreSQL integration tests for migrations, isolation, constraints, transactions, and concurrency.
- Preserve unrelated user changes and do not commit secrets.
