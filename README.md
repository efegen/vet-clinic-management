# Veterinary Clinic Appointment Management System

An internal management panel for small-to-medium veterinary clinics, built with **ASP.NET Core 8** and **Razor MVC**. Clinic staff (receptionists, veterinarians, managers) manage pet owners, pets, the service catalog, and appointments from a single dashboard — replacing paper calendars and ad-hoc spreadsheets.

> Academic project — Web Application Development course, İstanbul Beykent University, 2025–2026 Spring term.

---

## Table of Contents

- [The Problem](#the-problem)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Data Model](#data-model)
- [Business Rules](#business-rules)
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

- **Owner management** — full CRUD with unique phone validation and cascade-aware deletion.
- **Pet management** — species/breed/gender tracking, automatic age calculation ("3 years 2 months"), per-owner listing.
- **Service catalog** — duration and pricing, soft activate/deactivate, applicable-species targeting.
- **Appointments** — create/edit/cancel with:
  - Date validation (working hours, no Sundays, no past dates, duration must fit the workday).
  - **Conflict detection** so two appointments never overlap.
  - **Status workflow** (Pending → Confirmed → Completed / Cancelled) with enforced transitions.
  - **Price & end-time snapshots** so historical appointments stay accurate even after a service's price or duration changes.
- **Dashboard** — today's and this week's appointment counts, total owners/pets, pending count, today's schedule, upcoming 7-day list, and the most-requested services over the last 30 days.
- **Weekly calendar view** — appointments laid out across the week.
- **List utilities** — server-side search and pagination on all entity lists.
- **Responsive UI** — Bootstrap 5, colored status/species badges, flash messages.

---

## Technology Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8.0 |
| UI | Razor View Engine (MVC) |
| CSS Framework | Bootstrap 5.3 |
| ORM | Entity Framework Core 8 (Code First) |
| Database | SQLite |
| Validation | Data Annotations + jQuery Validation (client) + service-layer rules |
| Logging | Built-in `ILogger<T>` |

---

## Architecture

A clean, layered architecture with strictly top-down communication:

```
Controllers  →  Services  →  Repositories  →  DbContext  →  SQLite
```

| Layer | Responsibility |
|---|---|
| **Controller** | Handles HTTP requests, maps ViewModel ↔ Entity, calls services only. Never touches `DbContext`. Thin actions. |
| **Service** | Business logic: date validation, conflict checks, age calculation, status transitions. No HTTP, returns no views. |
| **Repository** | Data access. All LINQ queries live here. No business rules. |
| **DbContext** | EF Core `DbSet`s and relationship configuration (`OnModelCreating`). |

Key design decisions:

- **Dependency Injection** — every repository and service is registered against an interface (`IOwnerRepository`, `IAppointmentService`, …) with **Scoped** lifetime, matching the `DbContext` lifetime to avoid captive-dependency issues.
- **Generic `IRepository<T>`** — common CRUD is defined once; entity-specific repositories add only their specialized queries (DRY).
- **ViewModel pattern** — entities are never passed to views, preventing over-posting and tight coupling.
- **Manual mapping** — ViewModel ↔ Entity conversion is hand-written (no AutoMapper) for transparency.
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
| **Appointment** | PetId, ServiceId, AppointmentDate, EndTime*, ServicePriceSnapshot*, Status (enum), Notes?, CreatedAt |

`*` **Snapshot fields** — `EndTime` and `ServicePriceSnapshot` are computed at create/update time from the service's current duration and price, then stored physically. This guarantees historical accuracy (a price change tomorrow doesn't rewrite yesterday's appointment) and keeps conflict queries as simple date comparisons that translate cleanly to SQLite.

Enums: `PetSpecies { Kopek, Kedi, Kus, Tavsan, Diger }`, `PetGender { Erkek, Disi }`, `AppointmentStatus { Beklemede, Onaylandi, Tamamlandi, IptalEdildi }`.

The `Owner.Phone` column has a **DB-level unique index** as a last line of defense against race conditions; the service layer catches the resulting `DbUpdateException` and returns a friendly error.

---

## Business Rules

- Appointments may only be booked during **working hours (09:00–19:00)**.
- **No appointments on Sundays.**
- **No past-dated** appointments.
- The service duration must **fit inside the workday** (end time cannot pass 19:00).
- **No overlapping appointments** — conflict detection uses the formula `A.start < B.end AND A.end > B.start`, ignoring cancelled appointments.
- **Status transitions** are enforced: `Pending → Confirmed | Cancelled`, `Confirmed → Completed | Cancelled`; `Completed` and `Cancelled` are terminal.
- **Cancellation is soft** — the appointment's status becomes `Cancelled` with a reason in Notes; the record is never physically deleted, so cancellation statistics survive.
- A service **with appointments cannot be deleted** — deactivate it instead (`IsActive = false`); it then disappears from new-appointment dropdowns but keeps its history.
- Deleting an **owner cascades** to its pets and all their appointments (clearly warned in the confirmation dialog).

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
├── ViewModels/         # View-specific models (List/Details/CreateEdit per entity, Dashboard)
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── Migrations/
│   └── SeedData.cs
├── Repositories/
│   ├── Interfaces/     # IRepository<T> + per-entity interfaces
│   └── Implementations/
├── Services/
│   ├── Interfaces/
│   ├── Implementations/
│   └── Result.cs       # Success/Fail result type for the service layer
├── Helpers/            # AgeCalculator, EnumDisplay, MoneyFormat, PhoneFormat
├── Views/              # Razor views (Shared layout + per-entity CRUD views)
├── wwwroot/            # Static assets (CSS, JS, Bootstrap, jQuery)
├── appsettings.json
└── Program.cs          # DI registration, middleware pipeline, migrate + seed
```

---

## Database & Migrations

The app uses SQLite with a file-based database (`vetclinic.db`, created automatically on first run via `db.Database.Migrate()` in `Program.cs`).

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

When the database is empty, `SeedData.Initialize` loads a demo dataset so the app is usable immediately:

- **3 owners**
- **6 services** (general exam, vaccination, spay/neuter, dental cleaning, nail trim, …)
- **5 pets** linked to those owners
- **4 future-dated appointments**

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
