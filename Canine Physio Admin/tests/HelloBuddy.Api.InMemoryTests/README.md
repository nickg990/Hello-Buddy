# HelloBuddy.Api.InMemoryTests

Fast API tests that run with EF Core InMemory provider.

These tests complement (not replace) the real MySQL integration tests.

## Run

`dotnet test tests/HelloBuddy.Api.InMemoryTests/HelloBuddy.Api.InMemoryTests.csproj --arch x86`

## Purpose

- Validate endpoint behavior quickly (status codes, validation, auth gate).
- Keep feedback loop fast when local MySQL is unavailable.
- Leave provider-specific verification to `HelloBuddy.Api.IntegrationTests`.
