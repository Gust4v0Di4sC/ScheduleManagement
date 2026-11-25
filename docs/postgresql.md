# PostgreSQL and migrations

PostgreSQL persistence is not configured in the current foundation. Its implementation is coordinated with the database issues in Linear rather than based on a temporary schema.

The agreed conventions are:

- EF Core and Npgsql compatible with .NET 10;
- one PostgreSQL database per environment;
- schema ownership by domain module;
- `lower_snake_case` database identifiers;
- module-owned mappings and migrations;
- `account_id` on multi-tenant records;
- compound foreign keys and uniqueness where tenant isolation requires them;
- connection strings from User Secrets in Development and external configuration elsewhere.

When the initial model is approved, this document must record the exact design-time factory, migration commands, module migration order, clean-database verification, rollback procedure, and test connection setup.
