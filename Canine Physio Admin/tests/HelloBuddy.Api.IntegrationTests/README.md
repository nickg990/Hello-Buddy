# HelloBuddy.Api.IntegrationTests

These tests run against a real MySQL database and exercise Increment 2 API CRUD flows.

## Prerequisites

1. Local MySQL running with the `canine_physiotherapy` schema and seed scripts applied.
2. Set one of the following environment variables with a valid connection string:

- `HELLOBUDDY_TEST_DB_CONNECTION` (preferred)
- `ConnectionStrings__CaninePhysioDb`
- `MYSQLCONNSTR_CaninePhysioDb`

If none are set, tests fall back to this local default:

`Server=localhost;Port=3306;Database=canine_physiotherapy;User=root;Password=devroot;SslMode=None`

## Run

`dotnet test tests/HelloBuddy.Api.IntegrationTests/HelloBuddy.Api.IntegrationTests.csproj --arch x86`

`--arch x86` is currently required on this machine because x64 .NET 9 runtime is missing.