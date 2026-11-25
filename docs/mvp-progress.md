# MVP implementation progress

This document separates implemented code from full Linear acceptance. An issue is not complete when a required endpoint, PostgreSQL migration, constraint, concurrency test, or interface decision is still pending.

## Implemented foundation

- WOR-95: modular host structure, environment configuration, safe global errors, OpenAPI, health checks, architecture tests, CI and documentation.
- Existing `/health/live` and `/openapi/v1.json` behavior is preserved and tested.

## Partial API work with unit-test evidence

- E1: tenant context contracts and outbox/inbox dispatch behavior for WOR-97 and WOR-99; security middleware for WOR-101.
- E2: account, contract, invitation, subdomain and publication rules for WOR-103, WOR-105, WOR-107 and WOR-109.
- E3: account-scoped users, roles, refresh-session rotation, customer CPF and consent for WOR-111, WOR-113 and WOR-115.
- E4: establishment schedules, catalog modalities, professional eligibility, storage abstraction and service policies for WOR-117 through WOR-125.
- E5: UTC ranges, advance windows, buffers, absences, daily availability and deterministic allocation for WOR-127 through WOR-131.
- E6–E8: quote snapshots, appointment aggregate, idempotent creation orchestration, lifecycle and rescheduling rules for WOR-133 through WOR-149.
- E9: payment model and simulated gateway, notification templates/retries/captured mailbox and masked audit records for WOR-151, WOR-153 and WOR-155.
- E10: isolated local export artifacts and expiry cleanup for the storage portion of WOR-159.

## Not yet accepted

- EF Core/Npgsql mappings, migrations, constraints and PostgreSQL integration tests await the corresponding `[DB]` contracts.
- JWT authentication endpoints and persisted sessions await alignment with WOR-112, WOR-114 and WOR-116.
- Database-backed outbox recovery after process restart remains unproved until WOR-100 is available.
- PostgreSQL concurrency, performance and tenant-isolation evidence remains pending.
- WOR-157 awaits agreement with WOR-94 on `/publico/{site}` and its DTOs.
- WOR-161 awaits the complete persisted journey and pilot data.
