# OPD Manager — Claude Context File

> This file is for Claude. Read it at the start of every session to understand
> the full project context, current state, and what needs to be done next.

---

## Project Overview

A **WPF desktop application** for an Outpatient Department (OPD) Manager.
Fully offline, single-user, no network required.

**Purpose:** Help the OPD manager control daily operations:
- Manage doctors, patients, appointments
- Monitor patient queue and waiting times
- Track operational issues
- Generate reports (PDF, Excel, Print)

---

## Tech Stack

| Layer | Technology |
|---|---|
| UI | WPF (C#), MVVM pattern |
| Architecture | Clean Architecture (4 layers) |
| ORM | Entity Framework Core 10 |
| Database | SQLite (local file `opd.db`) |
| DI | Microsoft.Extensions.DependencyInjection |
| PDF export | QuestPDF |
| Excel export | ClosedXML |
| Target framework | .NET 10 / net10.0-windows |

---

## Solution Structure

```
E:\Projects\C Sharp\OPD System\
├── OpdManager.slnx
├── CLAUDE.md                          ← this file
├── OpdManager.Domain\                 ← entities + enums only, no dependencies
├── OpdManager.Application\            ← interfaces + DTOs + service implementations
├── OpdManager.Infrastructure\         ← EF Core, repositories, UoW, ReportService
└── OpdManager.Presentation\           ← WPF, ViewModels, Views, DI wiring
```

### Dependency Direction
```
Presentation → Infrastructure → Application → Domain
Presentation → Application (direct for service interfaces)
```

---

## Domain Layer — Entities & Enums

### Entities (`OpdManager.Domain/Entities/`)
- `Doctor` — Id, Name, Specialty, PhoneNumber, StartTime, EndTime, IsActive
- `Patient` — Id, FullName, PhoneNumber, DateOfBirth, Notes, CreatedAt
- `Appointment` — Id, AppointmentDate, **TimeSlotTicks** (long, NOT TimeSpan — SQLite fix), Status, Notes, CreatedAt, DoctorId, PatientId
  - `[NotMapped] TimeSpan TimeSlot` computed from `TimeSlotTicks`
- `QueueEntry` — Id, QueueNumber, Status, CheckInTime, StartTime?, EndTime?, AppointmentId
  - `[NotMapped] int? WaitingMinutes` computed property
- `Issue` — Id, Title, Description, Category, Status, ReportedAt, ResolvedAt?, Resolution?, AppointmentId?

### Enums (`OpdManager.Domain/Enums/`)
- `AppointmentStatus`: Scheduled, InProgress, Completed, Cancelled, NoShow
- `QueueStatus`: Waiting, InProgress, Done, Skipped
- `IssueStatus`: Open, InReview, Closed

### Critical: TimeSpan → SQLite Fix
SQLite cannot order/compare TimeSpan. Solution:
- Store as `long TimeSlotTicks` in DB
- Expose as `[NotMapped] TimeSpan TimeSlot` computed property
- Always order by `TimeSlotTicks` in LINQ queries

---

## Application Layer

### Interfaces (`OpdManager.Application/Interfaces/`)

**`IBaseRepository<T>`** — Query(), GetAllAsync(), GetByIdAsync(), GetWhereAsync(), AddAsync(), Update(), Delete()

**Repository interfaces** (all extend IBaseRepository):
- `IDoctorRepository` — + GetActiveAsync()
- `IPatientRepository` — + SearchAsync(term)
- `IAppointmentRepository` — + GetByDateAsync(), GetByDatePagedAsync(), SearchPagedAsync(), GetByPatientPagedAsync(), GetByDoctorAsync(), IsSlotAvailableAsync(), GetAllByDoctorAsync()
- `IIssueRepository` — + GetByStatusAsync(), SearchPagedAsync()
- `IQueueRepository` — + GetTodayQueueAsync(), GetNextQueueNumberAsync()

**`IUnitOfWork`** — Doctors, Patients, Appointments, Issues, Queue, SaveChangesAsync()

### DTOs (`OpdManager.Application/DTOs/`)
- `AppointmentDto` — Id, PatientId, DoctorId, PatientName, DoctorName, DoctorSpecialty, AppointmentDate, TimeSlot, Status, QueueNumber, WaitingMinutes, Notes
- `QueueEntryDto` — Id, QueueNumber, PatientName, DoctorName, DoctorSpecialty, Status, CheckInTime, WaitingMinutes, StartTime?, EndTime?
- `DashboardDto` — TotalPatientsToday, WaitingPatients, InProgressPatients, CompletedPatients, AverageWaitingMinutes, ActiveDoctors, OpenIssues
- `PagedResult<T>` — Items, TotalCount, Page, PageSize, TotalPages, HasNext, HasPrevious

### Service Interfaces (`OpdManager.Application/Services/Interfaces/`)
- `IDoctorService` — GetAllAsync, GetActiveAsync, GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync
- `IPatientService` — GetAllAsync, GetByIdAsync, SearchAsync, AddAsync, UpdateAsync, DeleteAsync
- `IAppointmentService` — GetByDateAsync, GetByDatePagedAsync, SearchPagedAsync, GetByPatientPagedAsync, GetByIdAsync, BookAsync, CancelAsync, UpdateStatusAsync, DeleteAsync
- `IQueueService` — GetTodayQueueAsync, CheckInAsync, UpdateStatusAsync, GetDashboardStatsAsync
- `IIssueService` — SearchPagedAsync, GetAllAsync, GetOpenAsync, AddAsync, UpdateAsync, MarkInReviewAsync, CloseAsync, DeleteAsync
- `IReportService` — ExportAppointmentsToPdf, ExportAppointmentsToExcel (NO print — print is WPF only)

### Service Implementations (`OpdManager.Application/Services/Implementation/`)
All services live in Application because they only depend on `IUnitOfWork`.
Exception: `ReportService` lives in Infrastructure (depends on QuestPDF, ClosedXML).

---

## Infrastructure Layer

### Repositories (`OpdManager.Infrastructure/Repositories/`)
- `BaseRepository<T>` — implements `IBaseRepository<T>`, all shared CRUD
- All concrete repos extend `BaseRepository<T>` and only add unique methods
- `IssueRepository` — overrides `GetAllAsync()` with `new` keyword (ordered by ReportedAt desc)
- `QueueRepository` — overrides `Delete()` with `new` keyword

### UnitOfWork (`OpdManager.Infrastructure/UnitOfWork/UnitOfWork.cs`)
- Receives all repositories via DI constructor injection (NOT `new`-ing them)
- Shares same `AppDbContext` instance across all repos (scoped lifetime)

### DbContext (`OpdManager.Infrastructure/Data/AppDbContext.cs`)
- `AppDbContextFactory` also exists for EF CLI migrations (design-time only)
- Connection string: `Data Source=opd.db`
- `Appointment` entity: `Ignore(a => a.TimeSlot)` — uses TimeSlotTicks instead

### Services (`OpdManager.Infrastructure/Services/`)
- `ReportService` — QuestPDF for PDF, ClosedXML for Excel, opens file after export

---

## Presentation Layer

### DI Registration (`App.xaml.cs`)
```
DbContext          → AddDbContext (Scoped)
Repositories       → AddScoped (all 5)
UnitOfWork         → AddScoped
Services           → AddScoped (all 6 including ReportService)
ViewModels         → AddScoped (most), AddTransient (dialog VMs)
MainViewModel      → Manual factory (needs IServiceProvider injected)
MainWindow         → AddTransient
```

**Transient ViewModels** (fresh instance per dialog open):
- `AddEditDoctorViewModel`
- `AddEditAppointmentViewModel`
- `AddEditIssueViewModel`
- `CreatePatientViewModel`
- `PatientHistoryViewModel`

### Navigation Pattern
- `MainViewModel` owns `CurrentView` (BaseViewModel) + `ActiveNav` (string)
- `DataTemplate` in `MainWindow.xaml` maps ViewModel type → View
- All dialog subscriptions (`OpenDoctorDialog`, `OpenAppointmentDialog`, etc.) are wired in `MainViewModel` constructor — **never in the View** (prevents duplicate event handler bug)
- Dialog events use named local functions for proper `+=` / `-=` cleanup

### MVVM Core (`Presentation/Core/`)
- `BaseViewModel` — INotifyPropertyChanged, SetProperty, OnPropertyChanged
- `RelayCommand` — ICommand with RaiseCanExecuteChanged()
- `RelayCommand<T>` — generic version for typed parameters

### Converters (`Presentation/Converters/`)
- `TimeSpanTo12HourConverter` — `h:mm tt` format (no leading zero)
- `StatusToColorConverter` — AppointmentStatus → SolidColorBrush
- `StatusToTextConverter` — AppointmentStatus → "● Scheduled" etc.
- `QueueStatusToColorConverter` — QueueStatus → SolidColorBrush
- `QueueStatusToTextConverter` — QueueStatus → "⏳ Waiting" etc.

### Helpers (`Presentation/Helpers/`)
- `PrintHelper` — WPF print using FixedDocument + Grid rows (NOT FlowDocument Table — .NET 10 doesn't support TableColumnGroup)

---

## Screens / Views

| Screen | ViewModel | Key Features |
|---|---|---|
| Dashboard | DashboardViewModel | 7 stat cards, auto-refresh via IQueueService |
| Doctors | DoctorViewModel | DataGrid, Add/Edit dialog, Toggle Active, Delete (blocks if has appointments) |
| Patients | PatientViewModel | Search (Enter key), pagination 20/page, View History (double-click), New Patient |
| Appointments | AppointmentViewModel | Date mode + Search mode, pagination, doctor filter, Check In, Cancel, Delete |
| Queue Monitor | QueueMonitorViewModel | 3-column Kanban (Waiting/InProgress/Done), auto-refresh timer 30s, Call Next/Done/Skip |
| Issues | IssueViewModel | Search + category + status filter, pagination, Mark In Review / Close / Delete |
| Reports | ReportsViewModel | Date range picker, PDF/Excel/Print buttons |

---

## Key Business Rules

### Appointment Booking
- One slot per doctor per time — `IsSlotAvailableAsync` checks by `TimeSlotTicks`
- Edit excludes current appointment from slot check (`excludeAppointmentId`)
- Patient must be selected from search results — typing alone doesn't set `SelectedPatient`

### Queue / Status Sync
```
Check In     → QueueEntry created, Status = Waiting    | Appointment stays Scheduled
Call Next    → QueueStatus = InProgress                | Appointment = InProgress
Mark Done    → QueueStatus = Done                      | Appointment = Completed
Skip         → QueueStatus = Skipped                   | Appointment = NoShow
Cancel Appt  → AppointmentStatus = Cancelled           | QueueEntry.Status = Skipped
```

### Issue Workflow
```
Log Issue → Open
Open → Mark In Review → InReview
InReview → Close (enter resolution) → Closed
Open → Close (enter resolution) → Closed (can skip review)
```

### Doctor Delete Protection
- Cannot delete a doctor who has any appointments (any date)
- Throws `InvalidOperationException` with friendly message
- Caught in `DoctorViewModel.DeleteAsync` and shown as MessageBox

---

## Known Patterns & Decisions

### EF Tracking Conflict Fix
Never create a new entity with an existing Id and call Update — always fetch the tracked entity first, then modify its properties.

### StackOverflow Fix (Doctor filter ComboBox)
`_doctorsLoaded` flag prevents reloading doctors collection on every filter change, which would reset ComboBox selection and re-trigger the setter infinitely.

### Dialog Reopening Bug Fix
Dialog events (`OpenDoctorDialog` etc.) are subscribed ONCE in `MainViewModel` constructor. WPF `DataTemplate` creates new View instances on every navigation — subscribing in the View causes accumulation.

### Pagination
- Page size: 20 rows everywhere
- `PagedResult<T>` — Items, TotalCount, Page, PageSize, TotalPages, HasNext, HasPrevious
- Appointment search uses `SearchPagedAsync` — all filters applied at DB level (SQL WHERE + SKIP + TAKE)
- Patient pagination is client-side (fetch all, paginate in memory) — acceptable since patients grow slowly

### Print
Uses `FixedDocument` + `Grid` rows — NOT `FlowDocument` + `Table`. Reason: `TableColumnGroup` does not exist in .NET 10 WPF (`System.Windows.Documents` namespace missing it).

---

## Migrations History
1. `InitialCreate` — all tables
2. `fixTimeSlot` / `AddTimeSlotTicks` — replaced TimeSpan column with long (ticks)

Run migrations:
```powershell
cd "E:\Projects\C Sharp\OPD System\OpdManager.Infrastructure"
dotnet ef migrations add <Name> --startup-project ../OpdManager.Presentation/OpdManager.Presentation.csproj
dotnet ef database update --startup-project ../OpdManager.Presentation/OpdManager.Presentation.csproj
```

`db.Database.Migrate()` runs automatically on app startup — no need for `update-database` in production.

---

## NuGet Packages

### OpdManager.Infrastructure
- `Microsoft.EntityFrameworkCore` 10.0.x
- `Microsoft.EntityFrameworkCore.Sqlite` 10.0.x
- `Microsoft.EntityFrameworkCore.Design` 10.0.x (PrivateAssets=all)
- `Microsoft.EntityFrameworkCore.Tools` 10.0.x (PrivateAssets=all)
- `QuestPDF` — PDF generation
- `ClosedXML` — Excel generation

### OpdManager.Presentation
- `Microsoft.Extensions.DependencyInjection` 10.0.x
- `Microsoft.EntityFrameworkCore.Design` 10.0.x
- `Microsoft.VisualBasic` — InputBox for issue resolution dialog

---

## What Has Been Completed

- ✅ Phase 1 — Solution structure, Domain entities, EF Core + SQLite, Repositories, UoW, DI
- ✅ Phase 2 — Dashboard screen with 7 live stat cards
- ✅ Phase 3 — Doctors CRUD (Add/Edit/Delete/Toggle Active, working hours, 12-hour time display)
- ✅ Phase 4 — Appointments (full CRUD, slot validation, Check In, queue sync, status badges)
- ✅ Phase 5 — Queue Monitor (Kanban 3-col, auto-refresh), Issues (full workflow), Reports (PDF/Excel/Print)
- ✅ Pagination — 20 rows/page on Appointments, Patients, Issues with DB-level queries
- ✅ Search — Patient name search (Enter key), appointment cross-date search, issue title/category/status filter
- ✅ Patient screen — list, search, history popup, Create Patient dialog
- ✅ Issue workflow — Open → In Review → Closed with resolution note
- ✅ Doctor delete protection — blocks if doctor has appointments
- ✅ Phase 6 — COMPLETE
  - Global exception handler (UI + background + async)
  - File logger (logs/opd-{date}.log)
  - Backup & Restore (SQLite file copy with SaveFileDialog/OpenFileDialog)
  - Dashboard auto-refresh every 60s with start/stop on nav
  - Waiting alert banner (configurable threshold from settings)
  - App Settings (hospital name, alert threshold, backup folder) saved to settings.json
  - Settings dialog accessible from sidebar
  - Hospital name shown in sidebar + PDF report header
  - Exit confirmation dialog
  - BackupService uses AppSettings.BackupFolder if set

---

## What Still Needs To Be Done

- [ ] UI/UX design polish — use Impeccable skill for full visual redesign
- [ ] Patient pagination move to DB-level (currently client-side)
- [ ] Input validation — phone format, date range checks
- [ ] Doctor search — already has SearchText live filter, works fine

---

## File Locations Quick Reference

```
Domain entities:          OpdManager.Domain\Entities\
Domain enums:             OpdManager.Domain\Enums\
Repository interfaces:    OpdManager.Application\Interfaces\Repositories\
IUnitOfWork:              OpdManager.Application\Interfaces\IUnitOfWork.cs
Service interfaces:       OpdManager.Application\Services\Interfaces\
Service implementations:  OpdManager.Application\Services\Implementation\
DTOs:                     OpdManager.Application\DTOs\
DbContext + Factory:      OpdManager.Infrastructure\Data\
Repository impls:         OpdManager.Infrastructure\Repositories\
UnitOfWork impl:          OpdManager.Infrastructure\UnitOfWork\
ReportService:            OpdManager.Infrastructure\Services\
App entry + DI:           OpdManager.Presentation\App.xaml.cs
Main window + nav:        OpdManager.Presentation\MainWindow.xaml
ViewModels:               OpdManager.Presentation\ViewModels\
Views:                    OpdManager.Presentation\Views\
Converters:               OpdManager.Presentation\Converters\
Helpers:                  OpdManager.Presentation\Helpers\
MVVM core:                OpdManager.Presentation\Core\
```

---

## How To Run

```powershell
cd "E:\Projects\C Sharp\OPD System"
dotnet build
dotnet run --project OpdManager.Presentation
```

Or open `OpdManager.slnx` in Visual Studio 2022 and press F5.

DB file (`opd.db`) is created automatically in the working directory on first run.
