# Local setup

## Requirements

- .NET SDK 10
- Visual Studio with ASP.NET Core support, or the `dotnet` CLI
- PostgreSQL for persistence work once the database foundation is added

## CLI

```powershell
dotnet restore Reservas.slnx
dotnet build Reservas.slnx --no-restore
dotnet test Reservas.slnx --no-build
dotnet run --project src/Reservas.Api/Reservas.Api.csproj --launch-profile http
```

Then request:

- `http://localhost:5272/health/live`
- `http://localhost:5272/openapi/v1.json`

## Visual Studio

Open `Reservas.slnx`, set `Reservas.Api` as the startup project, choose the HTTP or HTTPS launch profile, and run the project.

## Configuration

Non-sensitive defaults live in `appsettings.json`. Store development secrets with User Secrets and provide configuration externally in other environments. Do not commit connection strings, signing keys, provider credentials, or exported data.
