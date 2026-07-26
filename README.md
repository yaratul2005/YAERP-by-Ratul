# YAERP (Yet Another Enterprise Resource Planning) by Ratul

An enterprise-grade, high-performance, modular ERP suite designed natively for desktop environments using modern Clean Architecture, CQRS, and Domain-Driven Design (DDD).

![System Architecture Placeholder]

## Core Features
* **Inventory & Warehouse Management:** Multi-warehouse stock tracking, dynamic product categories.
* **Procure-to-Pay (Purchasing):** Vendor management, purchase orders, automated inventory receipts.
* **Order-to-Cash (Sales):** Customer management, sales orders, integrated inventory fulfillment.
* **Financial Accounting (General Ledger):** Double-entry bookkeeping, automated journaling, invoicing.
* **Human Resources (HR):** Employee lifecycle management and payroll processing integrated into the ledger.
* **Reporting:** High-performance QuestPDF invoice generation and ClosedXML Excel data exports.

## Tech Stack
* Language: C# 12+ / .NET 8 (or .NET 9)
* Presentation Layer: MVVM Toolkit
* CQRS Pipeline: MediatR & FluentValidation
* Persistence: Entity Framework Core with Global Query Filters (PostgreSQL)
* Logging & Telemetry: Serilog (Console, File)
* Unit Testing: xUnit, Moq, NetArchTest

## Prerequisites
* .NET 8 / .NET 9 SDK
* PostgreSQL (Optional, runs in-memory fallback out-of-the-box if connection string is missing)

## How to Build, Test, and Run

1. **Build the Solution** (Zero warnings enforced):
   ```bash
   dotnet build YAERP.slnx -c Release
   ```

2. **Run Tests**:
   ```bash
   dotnet test YAERP.slnx
   ```

3. **Run Application**:
   ```bash
   dotnet run --project src/YAERP.UI/YAERP.UI.csproj
   ```
   *Note: Upon startup, the database seeder automatically initializes the core Tenant, System Admin, Chart of Accounts, and base Unit of Measures.*

## Downloading & Installing
To get started with YAERP as an end-user:

1. Navigate to the **Releases** page on this GitHub repository.
2. Download the latest `YAERP-v1.0-win-x64.zip` asset.
3. Extract the ZIP file into your desired directory.
4. Run `YAERP.UI.exe`. No .NET installation is required because the application is fully self-contained!
5. Upon first execution, YAERP will automatically generate a local `yaerp_local.db` SQLite database inside `%LocalAppData%/YAERP/` if a PostgreSQL connection string isn't provided. Default admin and base data are seeded instantly.
