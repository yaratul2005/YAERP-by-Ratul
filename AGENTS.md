```markdown
# AGENTS.md — YAERP (Yet Another Enterprise Resource Planning)
> **Author & System Architect:** Ratul  
> **Target OS:** Windows Desktop (x64 / ARM64)  
> **Tech Stack:** C# | .NET 8 / .NET 9 | WinUI 3 / WPF | Entity Framework Core | PostgreSQL / SQL Server  
> **Architectural Style:** Clean Architecture | CQRS | Domain-Driven Design (DDD) | MVVM  

---

## 1. Executive Summary & Vision

**YAERP** is an enterprise-grade, high-performance, modular ERP suite designed natively for Windows environments. It is built to rival top-tier commercial ERP systems by providing sub-second query response times, robust offline resilience, multi-tenant database isolation, and transactional integrity across business modules.

### Mission
To deliver a desktop-native ERP system that maintains absolute code clarity, strict domain isolation, and high maintainability over years of operational evolution.

---

## 2. Technical Stack & Core Dependencies

Agents **must non-negotiably** stick to the specified tech stack below. No unauthorized dependencies or libraries should be introduced without developer approval.

* **Language:** C# 12+ / .NET 8 (or .NET 9)
* **Presentation Layer:** WinUI 3 (Windows App SDK) or WPF with `CommunityToolkit.Mvvm`
* **Data Layer:** Entity Framework Core (EF Core) + Dapper (for high-speed reporting/read paths)
* **Databases Supported:** PostgreSQL (Primary), Microsoft SQL Server, SQLite (for local caching)
* **Mediator / Pipeline Pattern:** `MediatR`
* **Validation:** `FluentValidation`
* **Logging & Telemetry:** `Serilog` (Sinks: File, PostgreSQL, OpenTelemetry)
* **Auto-Updating & Distribution:** Velopack / MSIX Packaging

---

## 3. High-Level Architecture & Layer Boundaries

The codebase follows **Clean Architecture** combined with **Domain-Driven Design (DDD)**. Dependency flow strictly goes inward towards the `Domain` layer.

```text
               ┌──────────────────────────────────────────┐
               │         YAERP.UI (WinUI 3 / WPF)        │
               └────────────────────┬─────────────────────┘
                                    │ (Depends on)
                                    ▼
               ┌──────────────────────────────────────────┐
               │            YAERP.Application             │
               │   (CQRS Handlers, DTOs, Validations)    │
               └──────────┬──────────────────────┬────────┘
                          │                      │
        (Depends on)      ▼                      ▼     (Depends on)
┌───────────────────────────────┐      ┌───────────────────────────────┐
│     YAERP.Infrastructure      │      │         YAERP.Domain          │
│ (EF Core, Security, Hardware) │──────► (Entities, Aggregates, Value  │
└───────────────────────────────┘      │    Objects, Domain Events)    │
                                       └───────────────────────────────┘

```

### Layer Rules for AI Agents

1. **`YAERP.Domain`**:
* Contains purely domain models, Value Objects, Domain Events, and Custom Domain Exceptions.
* **Zero framework dependencies** (No EF Core attributes, no UI packages, no JSON libraries).
* All entity identifiers must use **Strongly-Typed IDs** (e.g., `CustomerId(Guid Value)` instead of raw `Guid`).


2. **`YAERP.Application`**:
* Implements CQRS (Commands for write operations, Queries for read operations).
* Holds request validators, DTO mappings, and interface abstractions (`IApplicationDbContext`, `IUserRepository`).
* No direct database queries; all logic flows through MediatR pipeline behaviors.


3. **`YAERP.Infrastructure`**:
* Handles EF Core DbContext, DbMigrations, external hardware communications (receipt printers, barcode scanners), and background jobs.
* Responsible for audit logging execution and row-level multi-tenancy filtering.


4. **`YAERP.UI`**:
* Modern Windows MVVM architecture.
* **Absolute Zero Logic in `.xaml.cs` (Code-Behind)** except for standard view initializer logic.
* Uses UI Virtualization (`DataGrid` virtualization, async pagination) to handle hundreds of thousands of rows smoothly.



---

## 4. Complete ERP Module Specification

All functional implementations must map into one of the following isolated functional domains:

| Module | Core Domain Responsibilities & Entities |
| --- | --- |
| **Core & Security** | User, Role, Permission, Tenant, AuditLog, License, SystemSetting, Notification |
| **General Ledger & Finance** | Account, JournalEntry, FiscalYear, CostCenter, TaxRate, BankAccount, Currency |
| **Inventory Management** | Product/SKU, Warehouse, Location, StockMovement, UnitOfMeasure, ReorderPoint, Batch/Lot |
| **Procurement & Purchasing** | Vendor, PurchaseRequisition, PurchaseOrder, GoodsReceipt, VendorInvoice |
| **Sales & CRM** | Customer, Lead, Quotation, SalesOrder, DeliveryNote, CustomerInvoice, SalesRep |
| **Human Resources (HR)** | Employee, Department, Attendance, LeaveRequest, Payroll, SalaryStructure |

---

## 5. Development Guidelines for AI Agents

### A. C# Coding Conventions

* **Nullability:** `<Nullable>enable</Nullable>` is enforced. No code should introduce unhandled null warnings.
* **Immutability:** Value Objects and Data Transfer Objects (DTOs) must be immutable (e.g., `public record CustomerDto(...)`).
* **Concurrency Control:** All financial and inventory entities must include an optimistic concurrency token (`byte[] RowVersion` or `uint Version`).
* **Asynchronous Execution:** Every I/O bound call (DB access, File I/O, Network calls) must be `async` with `CancellationToken` support passed down through all parameters.

### B. CQRS & Database Patterns

* **Write Path (Commands):** Must execute within explicit transactional boundaries. Must trigger Domain Events (e.g., `InventoryDecrementedEvent`) handled post-commit.
* **Read Path (Queries):** Bypasses EF Core tracking overhead. Use `.AsNoTracking()` in EF Core or raw SQL via Dapper for complex reporting grids.
* **Multi-Tenancy:** Global Query Filters must be applied to `DbContext` automatically based on the current tenant context.

### C. UI & UX Performance Guidelines

* **Non-Blocking UI:** Heavy data loading operations must run off the main UI thread. Views must use skeleton loaders or async loading states.
* **Data Virtualization:** Grids displaying records (Invoices, Inventory logs) must use server-side pagination or UI virtualizing stacks. Never load 10,000+ items into memory at once.

---

## 6. Security & Audit Logging Rules

1. **Field-Level Encryption:** Sensitive fields (e.g., connection strings, API tokens, employee salary bases) must be encrypted using DPAPI or AES-256 before storage.
2. **Immutable Audit Trail:** Every Create, Update, and Delete operation across the system must emit an audit entry storing:
* `UserId`, `TenantId`, `Timestamp (UTC)`, `EntityName`, `ActionType`, `OldValues (JSON)`, `NewValues (JSON)`.


3. **Role-Based Access Control (RBAC):** Every UI View and Command Handler must be protected by explicit permission checks (e.g., `[AuthorizePermission("Inventory.Delete")]`).

---

## 7. Testing & Quality Assurance Architecture

Every generated module must be accompanied by relevant unit and integration tests:

* **Unit Tests (`YAERP.Domain.Tests`):** Test domain logic, state mutations, and business rules without external mocks.
* **Application Tests (`YAERP.Application.Tests`):** Test Command/Query handlers using in-memory SQLite or Moq wrappers.
* **Architecture Tests (`YAERP.Architecture.Tests`):** Uses `NetArchTest` to verify that layer dependency rules are strictly maintained (e.g., Domain doesn't reference Infrastructure).

---

## 8. Definition of Done (DoD) Checklist

When an AI Agent completes a task, it must auto-verify these constraints before marking the work complete:

* [ ] Code builds cleanly with zero compilation warnings (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`).
* [ ] No raw business logic exists in XAML code-behind files.
* [ ] Domain logic has corresponding Unit Tests.
* [ ] EF Core Migration file is generated cleanly (if entities changed).
* [ ] Database indexes are added for high-frequency query fields (e.g., Foreign Keys, Status fields, Dates).
* [ ] Proper error handling uses the custom Result pattern (`Result<T>`) instead of throwing unhandled runtime exceptions.

---

*Created by Ratul — Lead Architect & Creator of YAERP*

```

```
