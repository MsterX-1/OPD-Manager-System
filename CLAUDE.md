# OPD Manager — Claude Context File

> This file is for Claude. Read it at the start of every session to understand
> the full project context, current state, and what needs to be done next.

---

## Project Overview

A **Web-based application** for an Outpatient Department (OPD) Manager, currently expanding into a broader Hospital ERP system.
Architecture migrated from WPF/SQLite to ASP.NET Core Web API + React SPA + SQL Server.

**Purpose:** Help the hospital manage daily operations:
- Manage doctors, patients, appointments, and medical records
- Monitor patient queue and waiting times in real-time
- Handle intra-hospital referrals (Doctor to Doctor, Doctor to Department)
- Track operational issues
- Generate reports (PDF, Excel)
- System administration and user management

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend (SPA) | React, TypeScript, Vite, TanStack Query, Zustand, React Router |
| UI Design | Custom Premium Dark Theme (Vanilla CSS + Impeccable Refinements) |
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

## Roles & Access Control

| Role | Scope |
|---|---|
| **Admin** | Full system — Users, Doctors, Departments, Settings, all data, all reports |
| **Staff** | Front-desk/reception — patients, booking, check-in |
| **Doctor** | Only their own appointments, queue, and patients; writes/edits medical records and prescriptions; creates referrals |
| **DepartmentStaff** | Only their own department's referral queue; updates status, attaches results |

---

## Domain Layer — Entities & Enums

### Core Entities (`OpdManager.Domain/Entities/`)
- `Doctor` — Id, UserId (nullable), Name, Specialty, PhoneNumber, IsActive, AverageExaminationMinutes?
- `DoctorSchedule` — Id, DoctorId, DayOfWeek, StartTime, EndTime, IsWorking
- `Patient` — Id, FullName, PhoneNumber, DateOfBirth, Gender, NationalId, KnownAllergies, Notes, CreatedAt
- `Appointment` — Id, AppointmentDate, Status, Notes, CreatedAt, DoctorId, PatientId
- `QueueEntry` — Id, QueueNumber, Status, CheckInTime, StartTime?, EndTime?, AppointmentId
- `Issue` — Id, Title, Description, Category, Status, ReportedAt, ResolvedAt?, Resolution?, AppointmentId?
- `AppSetting` — Key, Value

### Clinical & Referral Entities (Phase A Expansion)
- `MedicalRecord` — Id, AppointmentId, DoctorId, ChiefComplaint, Vitals (BP, Temp, Pulse, Resp, Weight, Height, O2), Diagnosis, ClinicalNotes, CreatedAt, UpdatedAt
- `Prescription` — Id, MedicalRecordId, MedicationName, Dosage, Frequency, DurationDays, Instructions
- `Attachment` — Id, MedicalRecordId?, ReferralId?, FileType, FilePath, UploadedAt, UploadedByUserId, Description
- `Department` — Id, Name, Type, IsActive
- `DepartmentService` — Id, DepartmentId, Name, Fee
- `Referral` — Id, PatientId, ReferringDoctorId, SourceAppointmentId, ReferralType, TargetDoctorId?, TargetDepartmentId?, TargetDepartmentServiceId?, Reason, Status, CreatedAt, CompletedAt?, ResultNotes?, ResultAttachmentId?
- `DepartmentQueueEntry` — Id, QueueNumber, Status, CheckInTime, StartTime?, EndTime?, ReferralId

### Infrastructure Entities (`OpdManager.Infrastructure/Identity/`)
- `ApplicationUser` (extends IdentityUser) — FullName, DoctorId?, DepartmentId?
- `RefreshToken` — Token, UserId, ExpiresAt, IsRevoked

### Enums (`OpdManager.Domain/Enums/`)
- `AppointmentStatus`: Scheduled, InProgress, Completed, Cancelled, NoShow
- `QueueStatus`: Waiting, InProgress, Done, Skipped
- `ReferralType`: ToDoctor, ToDepartment
- `ReferralStatus`: Pending, InProgress, Completed, Cancelled

---

## React Frontend (`opdmanager-client`)

### Screens / Pages
| Route | Description | Role |
|---|---|---|
| `/login` | JWT Login form | Anonymous |
| `/` | Dashboard with stat cards & Recharts performance graph | Authenticated |
| `/queue` | Live Queue Kanban board (SignalR) | Authenticated |
| `/appointments` | Appointment management & check-in | Authenticated |
| `/patients` | Patient directory | Authenticated |
| `/patient-chart/:id` | Patient Chart (Medical history, referrals) | Authenticated |
| `/encounter/:id` | Medical Record form (vitals, diagnosis, rx) | Doctor / Staff |
| `/doctor-portal` | "My Queue" & "My Appointments" | Doctor |
| `/department-portal` | Department Queue & Result entry | DepartmentStaff |
| `/doctors` | Doctor CRUD + 7-day schedule editor | Admin |
| `/departments` | Department CRUD | Admin |
| `/issues` | Issue tracking | Authenticated |
| `/reports` | PDF/Excel exports | Authenticated |
| `/users` | System user management | Admin |
| `/settings` | Global application configuration | Admin |

---

## Key Business Rules

### Doctor Referrals
- Doctors can create referrals from the Encounter screen.
- `ToDoctor` creates a pending appointment suggestion for the target doctor.
- `ToDepartment` creates a `DepartmentQueueEntry` so the patient shows up in that department's Kanban board.
- When a department completes a referral, a SignalR notification alerts the referring doctor.

### Medical Records
- Vitals can be edited by Staff or Doctors.
- Diagnosis, Clinical Notes, and Prescriptions are strictly Doctor-only for the assigned appointment.

---

## What Has Been Completed
- ✅ **OPD Core**: Base system migrated from WPF to React + Web API.
- ✅ **UI Polish**: Fully themed and animated frontend using Impeccable standards (Harden, Onboard, Polish, Animate, Colorize).
- ✅ **Phase A (Core Clinical Loop)**: Added Doctor scoped access, Medical Records, Prescriptions, Attachments, Departments, and the complete Referral network logic.

## What Still Needs To Be Done
- [ ] **Phase B (Financial Loop)**: 
  - Payment method selection at booking.
  - Insurance providers and Patient policies.
  - `Invoice`, `InvoiceItem`, and `Payment` models.
  - Checkout/Billing screen and PDF invoice generation.
- [ ] **Phase C (Production Hardening)**: Soft deletes, Audit logs, secrets hygiene, concurrency tokens.
