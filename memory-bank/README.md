# AppIt Memory Bank

Living reference for how the AppIt hospitality platform is structured, how pages connect, and how data flows from the Angular UI through the .NET API to SQL Server.

## Documents

| Document | Purpose |
|----------|---------|
| [features/system-functionality-diagnosis.md](./features/system-functionality-diagnosis.md) | **Master domain inventory** — 17 functional areas, APIs, jobs, integrations, gap status |
| [keyWorkflows.md](./keyWorkflows.md) | Business workflows with mermaid diagrams and AppIt service references |
| [features/system-pages-and-booking-lifecycle.md](./features/system-pages-and-booking-lifecycle.md) | Page-by-page UI map — routes, APIs, permissions, booking lifecycle |
| [features/ComboProducts.md](./features/ComboProducts.md) | Combo catalog and booking splits |
| [features/ManageAgentRates.md](./features/ManageAgentRates.md) | Agent rate approval workflow |
| [features/auto-credit-note-after-fiscalization.md](./features/auto-credit-note-after-fiscalization.md) | Fiscal amend / refiscalization |
| [features/migration-cutover.md](./features/migration-cutover.md) | Optional migration and sync tooling |

## Local URLs

| Surface | URL |
|---------|-----|
| Web UI | http://localhost:4200 |
| API | http://localhost:5175 |
| Swagger (Development) | http://localhost:5175/swagger |

Default admin seed: `admin@appit.com` / `Admin@2026` (override via `APPIT_ADMIN_PASSWORD` or `Auth:SeedAdminPassword`).

## Stack overview

```
Browser (Angular 20, PrimeNG)
  → dev proxy /api → AppIt.Api (ASP.NET Core, JWT)
    → AppIt.Core (services, DTOs, Quartz jobs)
      → AppIt.Data (EF Core) → SQL Server
    → SmartInvoice.Vsdc (ZRA, optional)
    → AppIt.Report (RDLC, optional)
```

## GoldenDusk parity

AppIt targets full operational parity with GoldenDusk (booking spine, finance, fiscal, integrations). Gap status per domain is tracked in [system-functionality-diagnosis.md](./features/system-functionality-diagnosis.md).
