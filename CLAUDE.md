# OPD Manager — Claude Context File

> This file is for Claude. Read it at the start of every session to understand
> the full project context, current state, and what needs to be done next.

---

## Project Overview

A **Web-based application** for an Outpatient Department (OPD) Manager.
Architecture migrated from WPF/SQLite to ASP.NET Core Web API + React SPA + SQL Server.

**Purpose:** Help the OPD manager control daily operations:
- Manage doctors, patients, appointments
- Monitor patient queue and waiting times in real-time
- Track operational issues
- Generate reports (PDF, Excel)
- System administration and user management

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend (SPA) | React, TypeScript, Vite, TanStack Query, Zustand, React Router |
| UI Design | Custom Premium Dark Theme (Vanilla CSS + Glassmorphism) |
| API | ASP.NET Core Web API (.NET 10) |
| Real-time | SignalR (Queue & Dashboard updates) |
| Auth | ASP.NET Core Identity + JWT Bearer Tokens |
| ORM | Entity Framework Core 10 |
| Database | SQL Server |
| PDF export | QuestPDF |
| Excel export | ClosedXML |

---

## Solution Structure

```
E:\Projects\C Sharp\OPD System\
├── OpdManager.slnx
├── CLAUDE.md                          ← this file
├── OpdManager.Domain\                 ← entities + enums only, no dependencies
├── OpdManager.Application\            ← interfaces + DTOs + service implementations
├── OpdManager.Infrastructure\         ← EF Core, Identity, repositories, UoW, ReportService
├── OpdManager.Api\                    ← ASP.NET Core Web API, Controllers, SignalR Hubs
└── opdmanager-client\                 ← React SPA frontend (Vite)
```

### Dependency Direction
```
Api → Infrastructure → Application → Domain
Api → Application (direct for service interfaces)
Client (React) → Api (via HTTP/REST & SignalR)
```

---

## Domain Layer — Entities & Enums

### Entities (`OpdManager.Domain/Entities/`)
- `Doctor` — Id, Name, Specialty, PhoneNumber, IsActive, AverageExaminationMinutes?
- `DoctorSchedule` — Id, DoctorId, DayOfWeek, StartTime, EndTime, IsWorking
- `Patient` — Id, FullName, PhoneNumber, DateOfBirth, Notes, CreatedAt
- `Appointment` — Id, AppointmentDate, Status, Notes, CreatedAt, DoctorId, PatientId
- `QueueEntry` — Id, QueueNumber, Status, CheckInTime, StartTime?, EndTime?, AppointmentId
  - `[NotMapped] int? WaitingMinutes` computed
  - `[NotMapped] int? ExaminationMinutes` computed
- `Issue` — Id, Title, Description, Category, Status, ReportedAt, ResolvedAt?, Resolution?, AppointmentId?
- `AppSetting` — Key, Value (DB-backed config replacing old JSON file)

### Infrastructure Entities (`OpdManager.Infrastructure/Identity/`)
- `ApplicationUser` (extends IdentityUser) — FullName
- `RefreshToken` — Token, UserId, ExpiresAt, IsRevoked

### Enums (`OpdManager.Domain/Enums/`)
- `AppointmentStatus`: Scheduled, InProgress, Completed, Cancelled, NoShow
- `QueueStatus`: Waiting, InProgress, Done, Skipped
- `IssueStatus`: Open, InReview, Closed

---

## Application Layer

### Service Interfaces (`OpdManager.Application/Services/Interfaces/`)
- `IDoctorService` — CRUD + GetScheduleAsync, SetScheduleAsync
- `IPatientService` — CRUD + Search
- `IAppointmentService` — CRUD + BookAsync (validates schedule & daily cap)
- `IQueueService` — Queue ops, CheckIn, DashboardStats, GetDoctorPerformanceAsync
- `IIssueService` — Issue workflow
- `IReportService` — PDF/Excel generation (returns byte[])
- `ISettingsService` — Get/Update settings from DB

---

## Infrastructure Layer

### DbContext (`OpdManager.Infrastructure/Data/AppDbContext.cs`)
- Extends `IdentityDbContext<ApplicationUser, IdentityRole, string>`
- SQL Server provider (`UseSqlServer`)
- TimeSpan maps natively to `time(7)` (no ticks workaround needed)
- `AppDbContextFactory` provided for EF CLI design-time migrations

---

## API Layer (`OpdManager.Api`)

### Controllers & Auth
- **JWT Auth**: `AuthController` provides Login and Refresh Token. Registration is disabled for anonymous users (only Admin can create users via `UsersController`).
- **Roles**: "Admin" and "Staff". 
  - Admin: manages Users, Doctors, Settings. 
  - Staff: standard day-to-day operations.

### Key API Endpoints
- `/api/auth/*` — Login, Refresh, Me
- `/api/users/*` — User CRUD (Admin)
- `/api/doctors/*` — Doctor CRUD + Schedule management (Admin)
- `/api/patients/*` — Patient CRUD (Any)
- `/api/appointments/*` — Book, Update, Cancel, Search (Any)
- `/api/queue/*` — Check-in, Update Status (Any)
- `/api/dashboard` — Live stats (Any)
- `/api/reports/*` — PDF/Excel downloads
- `/api/settings/*` — App configuration (Admin)

### SignalR Hubs
- `/hubs/queue` — Pushes `QueueUpdated` event when status changes or check-ins happen.
- `/hubs/dashboard` — Pushes `DashboardUpdated` event alongside queue changes.

---

## React Frontend (`opdmanager-client`)

- **State Management**: TanStack Query (server state), Zustand (client auth state).
- **HTTP Client**: Axios with JWT interceptor for auto-attaching tokens and refreshing on 401.
- **Styling**: Premium dark theme via `index.css`, glassmorphism, responsive grid layouts.

### Screens / Pages
| Route | Description | Role |
|---|---|---|
| `/login` | JWT Login form | Anonymous |
| `/` | Dashboard with stat cards & Recharts performance graph | Authenticated |
| `/queue` | Live Queue Kanban board (SignalR) | Authenticated |
| `/appointments` | Appointment management & check-in | Authenticated |
| `/patients` | Patient directory | Authenticated |
| `/doctors` | Doctor CRUD + 7-day schedule editor | Admin |
| `/issues` | Issue tracking | Authenticated |
| `/reports` | PDF/Excel exports | Authenticated |
| `/users` | System user management | Admin |
| `/settings` | Global application configuration | Admin |

---

## Key Business Rules & New Features

### Day-based Appointments & Daily Cap
- Appointments are now **day-based**, not tied to a specific time slot.
- `DoctorSchedule` defines working hours per weekday.
- **Daily Cap Feature**: If enabled in Settings, the system calculates a doctor's max capacity based on `(WorkingMinutes / AverageExaminationMinutes)`. `BookAsync` validates against this cap before allowing a new appointment.

### Real-time Sync
- Old WPF used a 30s timer. The React app connects via SignalR. When an appointment is checked in, all clients instantly update the Queue Monitor and Dashboard.

### Doctor Performance Tracking
- `QueueEntry` tracks `CheckInTime`, `StartTime`, and `EndTime`.
- `ExaminationMinutes` is calculated for completed visits.
- `Dashboard` and `Reports` expose this data (avg & total examination times).

---

## Migrations & Database

1. Ensure SQL Server (or LocalDB) is running.
2. Connection string is configured in `OpdManager.Api/appsettings.json`:
   `Server=MSTERX\MSTERX;Database=OPD;User Id=sa;Password=MsterXx123;TrustServerCertificate=True;`

**Generate/Apply Migrations:**
```powershell
cd "E:\Projects\C Sharp\OPD System"
dotnet ef migrations add <Name> --project OpdManager.Infrastructure --startup-project OpdManager.Api
dotnet ef database update --project OpdManager.Infrastructure --startup-project OpdManager.Api
```

*Note: On first run, EF `MigrateAsync()` applies pending migrations and the API seeds the "Admin" & "Staff" roles plus the default `admin@opd.local` user.*

---

## How To Run

### 1. Run the Backend API
```powershell
cd "E:\Projects\C Sharp\OPD System"
dotnet run --project OpdManager.Api
```
- Starts on `http://localhost:5073`
- Swagger UI available at `http://localhost:5073/swagger`

### 2. Run the React Frontend
```powershell
cd "E:\Projects\C Sharp\OPD System\opdmanager-client"
npm run dev
```
- Starts on `http://localhost:5173`
- Default login: `admin@opd.local` / `Admin@123`

---

## What Has Been Completed (Migration)
- ✅ **Domain/Application Changes**: Removed `TimeSlotTicks` workaround. Added `DoctorSchedule` entity. Added `ExaminationMinutes` and daily cap logic.
- ✅ **Infrastructure Swap**: Migrated from SQLite to SQL Server. Replaced local JSON settings with DB-backed `AppSetting`. Added ASP.NET Core Identity. Removed WPF-specific BackupService and print logic.
- ✅ **API Layer**: Created `OpdManager.Api` with full JWT auth, controllers, and SignalR hubs.
- ✅ **Frontend**: Bootstrapped React SPA with Vite. Configured premium dark mode aesthetics. Implemented routing, auth guards, API client, and layout components. Created placeholder/functional pages for all features.

## What Still Needs To Be Done
- [ ] Flesh out the UI components inside the React pages (e.g., forms, modals, tables for Doctors, Users, Settings).
- [ ] Implement pagination controls on the React table components.
