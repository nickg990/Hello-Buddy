# HelloBuddy.Api

ASP.NET Core API host for the Canine Physio Admin backend. This project wires DI, configuration, endpoint routing, and infrastructure integrations.

## Run

```powershell
dotnet run --project src/HelloBuddy.Api/HelloBuddy.Api.csproj --launch-profile http
```

## Test

```powershell
dotnet test tests/HelloBuddy.Api.InMemoryTests/HelloBuddy.Api.InMemoryTests.csproj
dotnet test tests/HelloBuddy.Api.IntegrationTests/HelloBuddy.Api.IntegrationTests.csproj
```

## Release gate

From workspace root:

```powershell
./Infrastructure/runbooks/Invoke-ReleaseGate.ps1
```
