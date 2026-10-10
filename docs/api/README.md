# Testing the MOVOQ API

Keep the API running in Development:

```powershell
cd e:\MOVOQ\backend
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project MOVOQ.Api --launch-profile http
```

Base URL: `http://localhost:5014`

## Postman (recommended for manual checks)

1. Import `docs/api/MOVOQ.postman_collection.json`.
2. Collection variable `baseUrl` = `http://localhost:5014`.
3. Collection variable `token` = `test` (Development only).
4. Run **Identity → Exchange session** first, then Steps / Draws / Rewards / Admin.

Development auth is `Authorization: Bearer test` plus:

- `X-Test-Firebase-Uid`
- `X-Test-Email`
- `X-Test-Email-Verified: true`

Copy a draw or reward id from the list responses into `drawId` / `rewardId` before Buy ticket or Redeem.

You can also import the live OpenAPI spec from `http://localhost:5014/openapi/v1.json` (Development only).

## Automated tests

```powershell
cd e:\MOVOQ\backend
dotnet test MOVOQ.Tests/MOVOQ.Tests.csproj --filter "FullyQualifiedName~Unit"
```

Use Postman for request/response inspection. Use the unit tests for business rules (points, idempotency, stock).
