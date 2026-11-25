# Architecture

## Shape

The API is a modular monolith: `Reservas.Api` is the composition root, executable, and deployment unit. Capabilities live under `Modules/Identity`, `Accounts`, `Customers`, `Catalog`, `Professionals`, `Scheduling`, `Notifications`, and `Platform`.

Modules start as folders in the host assembly. A separate project is justified only when a concrete boundary must be enforced, such as a stable internal contract, an independently owned persistence lifecycle, or recurring forbidden dependencies.

Inside a module, use these layers only when code exists for them:

- `Presentation`: HTTP, input validation, authorization and response mapping.
- `Application`: use-case orchestration and integration ports.
- `Domain`: entities, value objects, invariants and domain events.
- `Infrastructure`: EF Core, PostgreSQL and external adapters.

Domain code cannot depend on HTTP, EF Core or providers. A module cannot access another module's `DbContext`.

## Cross-cutting behavior

Platform middleware adds correlation IDs and security headers. Unexpected failures are converted to safe Problem Details responses. Controllers are rate-limited while the liveness endpoint is not.

Multi-tenant data will derive `account_id` from the authenticated account context. RLS remains undecided; application filters, compound constraints and PostgreSQL isolation tests are still required.

## Public API

Administrative API versioning will be introduced with its first resource contract. The public-site backlog explicitly names `/publico/{site}` in WOR-39. Its subroutes and host-resolution behavior must be aligned with the interface issue WOR-94 before the contract is stabilized.
