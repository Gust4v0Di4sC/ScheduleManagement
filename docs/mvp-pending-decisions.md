# MVP pending decisions

## Coordination required

- Align account-scoped identity with WOR-112, WOR-114, and WOR-116 before identity migrations are finalized.
- Align `/publico/{site}`, its subroutes, DTOs, cache semantics and host resolution with interface issue WOR-94 before implementing WOR-157.
- Approve the reserved-subdomain list and the site-publication checklist.
- Define payment, cancellation, advance-notice and rescheduling policies.
- Define audited operations and masked fields.
- Define media limits, export retention and notification-template content.
- Decide whether PostgreSQL RLS will be defense in depth.
- Define the data volume and environment used for the 30-day availability performance target.

## Current implementation blockers

- PostgreSQL setup, mappings and migrations await the corresponding database contracts.
- Authentication persistence awaits the database identity model.
- Public API contract stabilization awaits interface alignment.

These blockers do not prevent domain modeling, application contracts, middleware, unit tests, documentation or CI work.
