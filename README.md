# Smart Warehouse Management System

MVP foundation for an automated warehouse management system. The solution targets .NET 8 LTS and uses ASP.NET Core MVC, Razor, Bootstrap 5, EF Core, SQL Server, Identity, Serilog, Swagger, and SignalR.

## Projects

- `SmartWarehouse.Domain`: warehouse entities and business states.
- `SmartWarehouse.Application`: use case contracts and DTOs.
- `SmartWarehouse.Infrastructure`: EF Core SQL Server, migrations, seed data, RFID event handling, and stock operations.
- `SmartWarehouse.Web`: MVC interface, authenticated API, Swagger UI, SignalR hub, and backend controlled simulation.
- `SmartWarehouse.Tests`: stock, idempotency, reservation, and RFID unit tests.

## Local setup

1. Install the .NET 9 SDK (the project targets .NET 8) and SQL Server Express or SQL Server.
2. Open `SmartWarehouse.sln` in Visual Studio 2022 with the **ASP.NET and web development** workload, or use PowerShell in the solution directory.
3. Review `SmartWarehouse.Web/appsettings.Development.json` and set the SQL Server instance that is installed locally.
4. Restore the EF command line tool and create/update the database:

   ```powershell
   dotnet tool restore
   dotnet ef database update --project SmartWarehouse.Infrastructure --startup-project SmartWarehouse.Web
   ```

5. Run `dotnet run --project SmartWarehouse.Web`.

In Development, a background startup service also applies pending migrations and creates the roles. The local demo account is `admin@smartwarehouse.local` / `Admin123!`; change `Seed:AdminPassword` before sharing the app. Production does not run the development seed.

The seeded demo product is SKU `DEMO-001`, EPC `E2806894A000000000000001`, and location `A-01-01`. The simulation reads and validates this EPC, then records inbound or outbound stock movements. Receipt, reservation, and dispatch each require an idempotency key. Inventory uses SQL Server rowversion for optimistic concurrency.

The simulation is software only. Reader, conveyor, and AGV status are simulated data; no real device protocol or safety function is claimed. Emergency stop must remain implemented by the hardware safety circuit and controller.

## Tests

```powershell
dotnet test SmartWarehouse.sln
```
