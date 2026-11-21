# StockPilot — Clean Architecture refactor

This repository is a structural and dependency-boundary refactor of the uploaded Inventory ASP.NET Core application. The existing SQLite database file and old provider-specific EF migration history are intentionally excluded. A fresh database and a new migration baseline must be created after reviewing the model.

## Solution layout

- `StockPilot.Domain`: business entities and domain enums; no infrastructure dependencies.
- `StockPilot.Application`: DTOs, service contracts, repository abstractions, application exceptions, and feature use cases for categories, users, products and roles.
- `StockPilot.Infrastructure`: EF Core persistence, repository implementations, authentication implementation, and adapters for caching and persistence.
- `StockPilot.Api`: HTTP controllers, middleware, API configuration and composition root.
- `tests`: xUnit unit and integration test projects. Unit tests now include role-use-case tests; integration tests remain a placeholder until the new database/API test host is configured.
- `frontend`: reserved for the Next.js + TypeScript client.

## Database naming

The connection string key remains `Inventory` and the DbContext remains `InventoryDbContext` to preserve the existing database identity used throughout the original code. The current local default is `Data Source=Inventory.db`. No database file is included and startup does not automatically migrate or seed. Later, this connection string can be replaced with the Supabase PostgreSQL connection string and the provider switched to Npgsql.

## First run

Requires the .NET 10 SDK.

```powershell
dotnet restore .\StockPilot.sln
dotnet build .\StockPilot.sln
dotnet test .\StockPilot.sln
```

Do not run `Database.Migrate()` automatically at application startup. After reviewing the entities and configuration, create a new initial migration against a development database: 

```powershell
dotnet ef migrations add InitialStockPilot -p .\src\StockPilot.Infrastructure -s .\src\StockPilot.Api -o Persistence/Migrations
```

The application intentionally does not run migrations or admin seeding during startup. Configure JWT secret material and seed credentials through .NET user-secrets or environment variables before exposing the API.

## Known legacy gaps and next refactoring work

1. The uploaded source had an empty `SaleService` class and no sale/sale-item service implementations or corresponding API controllers. The entities and repository contracts are retained, but those use cases are not implemented by this refactor. The category delete operation was completed as a soft delete because the entity already has an `IsActive` flag.
2. `AuthService` remains Infrastructure-hosted because the legacy implementation combines EF Core persistence, BCrypt password hashing, JWT construction and refresh-token persistence. The next authentication refactor should introduce Application-facing persistence, password-hashing and token-issuance abstractions.
3. The product use case now lives in Application and uses an Application-owned cache contract; the HybridCache adapter lives in Infrastructure. Product validation has been cleaned up to reject non-positive prices, negative quantities and selling prices below cost.
4. Existing service methods and repository methods need cancellation-token propagation and consistent async conventions reviewed.
5. Multi-tenancy (`Business`, `BusinessUser`, tenant-scoped query enforcement) is not introduced in this structural pass. It must be designed before onboarding multiple businesses.
6. Swagger/OpenAPI package APIs and the solution must be verified with the .NET 10 SDK on the developer machine. The build could not be executed in the preparation environment because the .NET CLI is unavailable.

## Security notes

- No original `appsettings*.json` secrets were copied.
- Set `AppSettings:SecretKey` to a cryptographically random secret of at least 64 bytes for HS512.
- Do not commit user-secrets, connection strings, passwords, or production credentials.
- Admin seeding is not executed automatically.


## Clean Architecture refinement (November 2026)

The feature services are organized by use case in Application:

- `Application/Features/Categories/CategoryService.cs`
- `Application/Features/Users/UserService.cs`
- `Application/Features/Products/ProductService.cs`
- `Application/Features/Roles/RoleService.cs`

Product use cases now depend on the Application-owned `IApplicationCache` abstraction. `HybridApplicationCache` is the Infrastructure adapter around `HybridCache`. Role use cases depend on `IRoleRepository`; `RoleRepository` is the EF Core implementation. The role-list endpoint now retrieves roles for the authenticated user rather than returning role names assigned across all users. Repository contracts remain in Application and their EF Core implementations remain in Infrastructure.

`AuthService` remains in Infrastructure in this pass because its current implementation directly combines EF Core persistence, BCrypt password hashing, JWT construction, refresh-token persistence, and configuration access. Moving it safely requires introducing separate Application-facing user/auth persistence, password hashing, and token-generation abstractions and adapting the API contract. It has not been falsely presented as fully separated.

No database file or EF migration was added. The configured connection-string key remains `Inventory`.

### Verification note
The solution's prior state was reported by the developer as building successfully. This refinement was prepared in an environment without the .NET SDK, so a fresh build/test could not be run here. Run `dotnet build .\StockPilot.sln` and `dotnet test .\StockPilot.sln` after extracting this archive. The new unit tests cover role creation, duplicate assignment prevention and user-scoped role retrieval.
