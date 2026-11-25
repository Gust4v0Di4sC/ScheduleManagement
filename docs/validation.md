# Validation

## Foundation

```powershell
dotnet build Reservas.slnx --no-restore
dotnet test Reservas.slnx --no-build
```

The HTTP smoke test must verify:

- `/health/live` returns `200` and `Healthy`;
- `/openapi/v1.json` returns `200` in Development;
- correlation and security headers are present;
- the removed WeatherForecast template does not appear in OpenAPI.

## Persistence work

Acceptance that depends on schema, migrations, constraints, isolation, transactions or concurrency must run against PostgreSQL. Unit tests with mocks do not replace that evidence.

The outbox integration test must persist an event with the business transaction, stop before successful consumption, restart against the same database, retry delivery, redeliver the message, and prove that an idempotent consumer produces one observable effect.
