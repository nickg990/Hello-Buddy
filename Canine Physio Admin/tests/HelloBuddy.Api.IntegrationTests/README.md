# HelloBuddy.Api.IntegrationTests

These tests run against a real MySQL database and exercise Increment 2 API CRUD flows.

## Prerequisites

1. Local MySQL running with the `canine_physiotherapy` schema and seed scripts applied.
2. Set one of the following environment variables with a valid connection string:

- `HELLOBUDDY_TEST_DB_CONNECTION` (preferred)
- `ConnectionStrings__CaninePhysioDb`
- `MYSQLCONNSTR_CaninePhysioDb`

If none are set, tests fall back to this local default:

`Server=localhost;Port=3306;Database=canine_physiotherapy;User=root;Password=P3nyf@n01;SslMode=None`

## Post-test reset behavior

Integration test teardown now resets the database back to schema + Day 1 + MSc seed scripts.

- For host DB tests (`ApiIntegrationTests`), reset runs when the test fixture is disposed.
- For testcontainer DB tests, reset also runs before container teardown for deterministic cleanup.

Optional override for reset/admin connection:

- `HELLOBUDDY_TEST_DB_RESET_CONNECTION`

If not set, reset uses the test connection details but forces `User=root` and clears `Database` so the full rebuild scripts can run.

## Run

`dotnet test tests/HelloBuddy.Api.IntegrationTests/HelloBuddy.Api.IntegrationTests.csproj --arch x86`

`--arch x86` is currently required on this machine because x64 .NET 9 runtime is missing.

## Azurite lane (CR003-I3)

An emulator-backed blob-storage test lane is available for local parity with Azure Blob workflows.

1. Start Azurite (the local stack script now starts it by default).
2. Set `HELLOBUDDY_RUN_AZURITE_TESTS=true`.
3. Optional: set `HELLOBUDDY_AZURITE_CONNECTION` if not using `UseDevelopmentStorage=true`.

Then run the same test command. The Azurite integration test will run only when
`HELLOBUDDY_RUN_AZURITE_TESTS=true`.
