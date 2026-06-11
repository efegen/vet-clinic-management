# Veterinary Clinic Appointment Management System

An internal management panel for small-to-medium veterinary clinics, built with **ASP.NET Core 8** and **Razor MVC**. Clinic staff (receptionists, veterinarians, managers) manage pet owners, pets, the service catalog, and appointments from a single dashboard — replacing paper calendars and ad-hoc spreadsheets.

> Academic project — Web Application Development course, İstanbul Beykent University, 2025–2026 Spring term.
> The full design document (in Turkish) lives in [vet-clinic-spec.md](vet-clinic-spec.md).

---

## Table of Contents

- [The Problem](#the-problem)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Data Model](#data-model)
- [Business Rules](#business-rules)
- [Localization & Formatting](#localization--formatting)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Database & Migrations](#database--migrations)
- [Seed Data](#seed-data)
- [Roadmap](#roadmap)
- [License](#license)

---

## The Problem

Small veterinary clinics usually run appointment scheduling on paper diaries, wall calendars, or simple spreadsheets. This causes recurring problems:

- **Double-booking** — two appointments accidentally land in the same time slot.
- **Fragmented pet history** — finding what service a pet received on its last visit means flipping through paper.
- **Scattered contact info** — phone numbers stored inconsistently; duplicate owner records.
- **No price standardization** — the same "Vaccination" service gets billed at different prices.
- **No reporting** — questions like "how many appointments this week?" or "most requested service?" require manually scanning records.

This system links the **owner → pet → service → appointment** chain in a single relational database and enforces business rules (conflict detection, date validation, status transitions) in the application layer.

---

## Features

- **Owner management** — full CRUD with unique-phone validation (enforced both in the service layer and by a DB index) and cascade-aware deletion.
- **Pet management** — species/breed/gender tracking, automatic age calculation ("3 yıl 2 ay" / "8 aylık"), and quick "add a pet to this owner" links.
- **Service catalog** — duration and pricing, soft activate/deactivate, and **per-species applicability** (a service like "Dental Cleaning" can be limited to dogs and cats).
- **Appointments** — create / edit / cancel / confirm / complete with:
  - **Date validation** — working hours (09:00–19:00), no Sundays, no past dates, and the service duration must fit inside the workday.
  - **Conflict detection** so two appointments never overlap.
  - A **status workflow** (Pending → Confirmed → Completed / Cancelled) with enforced transitions.
  - **Price & end-time snapshots** so historical appointments stay accurate even after a service's price or duration changes.
  - Two interchangeable views: a **weekly calendar** (time-grid laid out 09:00–19:00, week-to-week navigation, today highlighted, Sundays marked closed) and a **filterable list**.
- **Dashboard** — KPI strip (today / this week / total owners / total pets / pending), today's schedule, the upcoming 7-day list, the most-requested services over the last 30 days, and a Monday–Sunday **week-distribution** mini bar chart.
- **List utilities** — server-side **search, sortable columns, and pagination** on the Owners, Pets, and Appointments lists, plus date-range and status filters on appointments.
- **Responsive UI** — Bootstrap 5, colored status/species badges, and flash messages for every action.

---

## Technology Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8.0 |
| UI | Razor View Engine (MVC) |
| CSS Framework | Bootstrap 5.3 |
| Client validation | jQuery Validation + Unobtrusive |
| ORM | Entity Framework Core 8 (Code First) |
| Database | SQLite |
| Validation | Data Annotations + service-layer business rules |
| Logging | Built-in `ILogger<T>` |

The project targets `net8.0` with **nullable reference types** and **implicit usings** enabled. Per-species applicability uses an **EF Core 8 primitive collection** (`List<PetSpecies>`), persisted as JSON in a single column — no extra join table required.

---

## Architecture

A clean, layered architecture with strictly top-down communication:

```
Controllers  →  Services  →  Repositories  →  DbContext  →  SQLite
```

| Layer | Responsibility |
|---|---|
| **Controller** | Handles HTTP requests, maps ViewModel ↔ Entity, calls services only. Never touches `DbContext`. Thin actions. |
| **Service** | Business logic: date validation, conflict checks, age calculation, status transitions, snapshotting. No HTTP, returns no views. |
| **Repository** | Data access. All LINQ queries live here. No business rules. |
| **DbContext** | EF Core `DbSet`s and relationship configuration (`OnModelCreating`). |

Key design decisions:

- **Dependency Injection** — every repository and service is registered against an interface (`IOwnerRepository`, `IAppointmentService`, …) with **Scoped** lifetime, matching the `DbContext` lifetime to avoid captive-dependency issues.
- **Generic `IRepository<T>`** — common CRUD is defined once in `Repository<T>`; entity-specific repositories derive from it and add only their specialized queries (DRY).
- **ViewModel pattern** — entities are never passed to views, preventing over-posting and tight coupling.
- **Manual mapping** — ViewModel ↔ Entity conversion is hand-written (no AutoMapper) for transparency.
- **Shared list infrastructure** — `ListQueryParams` (search / sort / page) and `PagedResult<T>` feed a reusable `_Pager` partial, so every list page gets consistent search, sorting, and pagination.
- **`Result` type** — services return a small `Result` (success/fail + message) instead of throwing for expected validation failures; controllers surface the message via `ModelState` or `TempData`.
- **Async everywhere** — all database access uses `async`/`await` (`ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`).
- **CSRF protection** — every POST action uses `[ValidateAntiForgeryToken]`.

---

## Data Model

Four core entities:

```
Owner   1 ───< 0..*  Pet           (cascade delete)
Pet     1 ───< 0..*  Appointment   (cascade delete)
Service 1 ───< 0..*  Appointment   (restrict delete — soft-deactivate instead)
```

| Entity | Key fields |
|---|---|
| **Owner** | FullName, Phone (unique), Email?, Address?, CreatedAt |
| **Pet** | Name, Species (enum), Breed?, BirthDate, Gender (enum), Weight?, OwnerId |
| **Service** | Name, Description?, DurationMinutes, Price, IsActive, ApplicableSpecies |
| **Appointment** | PetId, ServiceId, AppointmentDate, EndTime\*, ServicePriceSnapshot\*, Status (enum), Notes?, CreatedAt |

`*` **Snapshot fields** — `EndTime` and `ServicePriceSnapshot` are computed at create/update time from the service's current duration and price, then stored physically. This guarantees historical accuracy (a price change tomorrow doesn't rewrite yesterday's appointment) and keeps conflict queries as simple date comparisons that translate cleanly to SQLite (DateTime arithmetic on `Service.DurationMinutes` could not be translated to SQL).

`Service.ApplicableSpecies` is a `List<PetSpecies>` stored as a JSON column via EF Core 8's primitive-collection support — it scopes a service to the species it can be booked for.

Enums (members kept ASCII; Turkish display text comes from `EnumDisplay`):

- `PetSpecies { Kopek, Kedi, Kus, Tavsan, Diger }`
- `PetGender { Erkek, Disi }`
- `AppointmentStatus { Beklemede, Onaylandi, Tamamlandi, IptalEdildi }`

The `Owner.Phone` column has a **DB-level unique index** as a last line of defense against race conditions; the service layer catches the resulting `DbUpdateException` and returns a friendly error.

---

## Business Rules

- Appointments may only be booked during **working hours (09:00–19:00)**.
- **No appointments on Sundays.**
- **No past-dated** appointments.
- The service duration must **fit inside the workday** (end time cannot pass 19:00, and cannot spill into the next day).
- **No overlapping appointments** — conflict detection uses the formula `A.start < B.end AND A.end > B.start`, ignoring cancelled appointments. (A second appointment for the *same pet* on the same day is allowed but logged as a warning.)
- **Status transitions** are enforced: `Pending → Confirmed | Cancelled`, `Confirmed → Completed | Cancelled`; `Completed` and `Cancelled` are terminal.
- **Cancellation is soft** — the appointment's status becomes `Cancelled` with the reason saved to Notes; the record is never physically deleted, so cancellation statistics survive.
- A service **with appointments cannot be deleted** — deactivate it instead (`IsActive = false`); it then disappears from new-appointment dropdowns but keeps its history. (An inactive service still in use on the appointment being edited is kept selectable so the user doesn't lose it.)
- Deleting an **owner cascades** to its pets and all their appointments (clearly warned in the confirmation dialog).

---

## Localization & Formatting

The UI is in **Turkish**. To keep the C# clean and culture-safe:

- Enum members are ASCII (`Kopek`, `Kus`, `Onaylandi`…); the diacritic display text ("Köpek", "Kuş", "Onaylandı") and the matching Bootstrap badge colors are produced by the `EnumDisplay` helper.
- The app runs under **`InvariantCulture`**, so HTML `number`/`date` inputs are parsed predictably (decimal point, ISO dates). Money, phone numbers, and dates are then formatted for display by hand (`MoneyFormat`, `PhoneFormat`).

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- (Optional) `dotnet-ef` global tool — only needed if you want to create new migrations:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### Run

1. Clone and enter the repository:
   ```bash
   git clone <repo-url>
   cd vet-clinic-management
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run --project VetClinic.Web
   ```

   On first launch the app automatically applies pending migrations and seeds the database — no manual `dotnet ef database update` step is required.

4. Open the app in your browser:
   - HTTPS: <https://localhost:7022>
   - HTTP: <http://localhost:5134>

---

## Project Structure

```
VetClinic.sln
VetClinic.Web/
├── Controllers/        # Thin MVC controllers (Home, Owners, Pets, Services, Appointments)
├── Models/
│   ├── Entities/       # Owner, Pet, Service, Appointment
│   └── Enums/          # PetSpecies, PetGender, AppointmentStatus
├── ViewModels/
│   ├── Owners/ Pets/ Services/ Appointments/   # List / Details / CreateEdit per entity
│   ├── Dashboard/      # DashboardViewModel, TopServiceItem, DayCountItem
│   └── Common/         # ListQueryParams, PagedResult<T>, ListViewModel, PagerModel
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── Migrations/
│   └── SeedData.cs
├── Repositories/
│   ├── Interfaces/     # IRepository<T> + per-entity interfaces
│   └── Implementations/ # Repository<T> base + per-entity repositories
├── Services/
│   ├── Interfaces/
│   ├── Implementations/
│   └── Result.cs       # Success/Fail result type for the service layer
├── Helpers/            # AgeCalculator, EnumDisplay, MoneyFormat, PhoneFormat
├── Views/              # Razor views (Shared layout, calendar + list, per-entity CRUD)
├── wwwroot/            # Static assets (CSS, JS, Bootstrap, jQuery)
├── appsettings.json
└── Program.cs          # DI registration, middleware pipeline, migrate + seed
```

---

## Database & Migrations

The app uses SQLite with a file-based database (`vetclinic.db`, created automatically on first run via `db.Database.Migrate()` in `Program.cs`). Two migrations ship with the project: the initial schema and the addition of `Service.ApplicableSpecies`.

Create a new migration after changing an entity:
```bash
dotnet ef migrations add <MigrationName> --project VetClinic.Web
dotnet ef database update --project VetClinic.Web
```

Reset the database (development only):
```bash
dotnet ef database drop -f --project VetClinic.Web
dotnet ef database update --project VetClinic.Web
```

---

## Seed Data

When the database is empty, `SeedData.Initialize` loads a realistic demo dataset so the dashboard, calendar, and reports are populated on first run:

- **10 services** (general exam, vaccination, spay/neuter, dental cleaning, nail trim, blood test, ultrasound, X-ray, …), each scoped to the species it applies to.
- **12 owners** with **22 pets** across all five species.
- **Dozens of appointments** spanning the previous three weeks (completed and cancelled), today's schedule, and the next two weeks (pending and confirmed) — so conflict checks, the weekly calendar, and the "most-requested services" report all have data to show.

---

## Roadmap

Possible future enhancements (out of scope for this version):

- Authentication & authorization (ASP.NET Core Identity)
- Public customer-facing portal
- SMS appointment reminders (e.g. Twilio)
- Excel / PDF report export
- Multi-room (multi-vet) scheduling
- Richer dashboard charts (Chart.js)

> Authentication is intentionally excluded: the system is assumed to run on a secure clinic LAN with no public interface.

---

## License

Academic project — free to use for educational purposes.
