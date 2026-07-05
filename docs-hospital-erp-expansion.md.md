# OPD Manager → Hospital ERP Expansion

## Context

This builds directly on the architecture already described in `CLAUDE.md`
(migrated to ASP.NET Core Web API + React + SQL Server). Read the existing
`CLAUDE.md` first — everything below is additive to that foundation, not a
replacement of it.

(Note: I wasn't able to browse `github.com/MsterX-1/OPD-Manager-System`
directly — GitHub blocks automated fetches — so this is built from the
`CLAUDE.md` you shared, which is detailed enough to work from directly. If
anything below doesn't match the actual code, that's the thing to correct
first.)

You explicitly asked for:
1. Cash/insurance ("credit") payment option at booking, with a hospital-
   maintained list of accepted insurance providers.
2. A patient file that doctors *and* staff build up during the visit.
3. Doctor-initiated referral — to another doctor, or to a utility/department
   (lab, x-ray, clinic, etc.).
4. A doctor login/account to manage their own patients and appointments.
5. An account for each utility department (lab, clinic, etc.) to manage its
   own work.

Below is all five, plus what a real production hospital deployment needs on
top of that — organized so you can see clearly what you asked for vs. what's
an addition, and accept or defer each piece. It's grouped into modules, then
given a build order at the end, since this is too much to build — or
review — as one undifferentiated pile.

---

## Roles (expanded from Admin/Staff)

| Role | Scope |
|---|---|
| **Admin** | Full system — Users, Doctors, Departments, Insurance Providers, Settings, all data, all reports |
| **Staff** | Front-desk/reception — patients, booking, check-in, payments/checkout |
| **Doctor** *(new)* | Only their own appointments, queue, and patients; writes/edits medical records and prescriptions for their own encounters; creates referrals |
| **DepartmentStaff** *(new)* | Only their own department's referral queue; updates status, attaches results |

`ApplicationUser` gains two nullable FKs: `DoctorId` (links a login to a
`Doctor` record) and `DepartmentId` (links a login to a `Department` record).
A user has at most one of these set, matching their role.

---

## Module A — Doctor accounts & scoped access

Directly answers ask #4.

- `Doctor` gains `UserId` (nullable FK to `ApplicationUser`) — not every
  doctor needs a login on day one, so this stays optional.
- New endpoints, scoped to the calling doctor via their JWT claims (no
  `doctorId` route parameter the client could tamper with):
  - `GET /api/doctors/me/appointments`
  - `GET /api/doctors/me/queue`
  - `GET /api/doctors/me/performance`
- Admin links/unlinks a login to a `Doctor` record from the existing Users
  screen — extend `UsersController`/`/users` rather than building a separate
  flow.
- Frontend: a "My queue" / "My patients" view for the Doctor role. This can
  reuse the existing Queue Monitor Kanban component, just scoped to `me`
  instead of showing every doctor.

---

## Module B — Patient medical record (the "patient file")

Directly answers ask #2 — with one adjustment: you said the doctor *or
staff* add to it, so this isn't doctor-only. In real OPD workflows, staff or
a nurse typically records vitals before the doctor sees the patient, and the
doctor adds diagnosis and treatment. The permission model below reflects
that split.

### New entities
```
MedicalRecord
  - Id
  - AppointmentId (FK, one-to-one — one visit, one record)
  - DoctorId
  - ChiefComplaint
  - BloodPressureSystolic?, BloodPressureDiastolic?, TemperatureC?, PulseRate?,
    RespiratoryRate?, WeightKg?, HeightCm?, OxygenSaturation?
  - Diagnosis
  - ClinicalNotes
  - CreatedAt, UpdatedAt

Prescription
  - Id, MedicalRecordId (FK)
  - MedicationName, Dosage, Frequency, DurationDays, Instructions

Attachment
  - Id, MedicalRecordId?, ReferralId?   (nullable — attaches to whichever produced it)
  - FileType, FilePath, UploadedAt, UploadedByUserId, Description
```

### Permission split
- **Vitals fields** (blood pressure, temperature, pulse, weight, height, O2):
  editable by **Staff or Doctor** — whoever is with the patient at that point
  in the visit.
- **ChiefComplaint, Diagnosis, ClinicalNotes, Prescription**: **Doctor-only**,
  and only the *assigned* doctor for that appointment (Admin can override).
  Other doctors can **read** a patient's history for continuity of care, but
  not edit someone else's entry — flag this if you want a looser or stricter
  rule; it's a real policy decision, not just a technical one (see "Worth
  confirming" at the end).

### Patient chart / timeline
- New aggregate endpoint, `GET /api/patients/{id}/chart` — every past
  `MedicalRecord` + `Prescription` + `Attachment` + `Referral` for a patient,
  ordered chronologically. This becomes the actual "patient file" the doctor
  opens.
- Frontend: an encounter screen (vitals form + diagnosis + notes +
  prescription line items + a "Refer" button) and a read-only Patient Chart
  view for browsing history.

### Two small additions worth doing now (cheap, clinically important)
- Add `NationalId` (or whatever unique legal identifier the hospital uses)
  and `Gender` to `Patient` — real medical/insurance records need these and
  they're missing from the current entity.
- Add `KnownAllergies` (plain text) to `Patient` — a one-field addition with
  outsized safety value; doctors should see this before prescribing.
- Add a lightweight `ConsentRecord` (or just `Patient.ConsentGivenAt` +
  `ConsentVersion` if you want to keep it simple), captured at registration.
  See the Egypt-specific note in Module E — this is close to a legal
  requirement, not just good practice, once enforcement lands.

---

## Module C — Referral network (doctor → doctor, doctor → department)

Directly answers asks #3 and #5. This is the biggest structural addition.

### `Department` — the generalized "utility"
Rather than hard-coding Lab/X-ray/Clinic as separate concepts, model them as
one flexible entity so adding a new utility later (physiotherapy, ECG,
dental) doesn't need new code:
```
Department
  - Id, Name, Type (enum: Lab, Radiology, Pharmacy, GeneralClinic, Other), IsActive

DepartmentService
  - Id, DepartmentId, Name, Fee   (e.g., "CBC" - 150, "Chest X-Ray" - 300)
```
`DepartmentService` doubles as the department's price list, which Module D
(billing) needs anyway.

### `Referral` — the order/task itself
```
Referral
  - Id, PatientId, ReferringDoctorId, SourceAppointmentId
  - ReferralType (enum: ToDoctor, ToDepartment)
  - TargetDoctorId?, TargetDepartmentId?, TargetDepartmentServiceId?
  - Reason
  - Status (enum: Pending, InProgress, Completed, Cancelled)
  - CreatedAt, CompletedAt?
  - ResultNotes?, ResultAttachmentId?
```
- `ToDoctor`: creates a pending appointment suggestion for the target doctor
  (the target doctor or reception confirms which day, consistent with the
  day-based/no-time-slot booking model already in place).
- `ToDepartment`: creates a `DepartmentQueueEntry` (below) so the patient
  shows up in that department's work queue.

### `DepartmentQueueEntry` — deliberately separate from `QueueEntry`
```
DepartmentQueueEntry
  - Id, QueueNumber, Status (Waiting, InProgress, Done, Skipped)
  - CheckInTime, StartTime?, EndTime?
  - ReferralId
```
This mirrors the existing `QueueEntry` shape rather than overloading it,
specifically to avoid touching the existing, working Doctor-Appointment
queue logic. The same Kanban-board UI pattern (Waiting/InProgress/Done
columns) the Queue Monitor already uses can be reused for department
screens.

### API & real-time
- `ReferralsController` — create, "my referrals as doctor", "queue for my
  department", update status, attach result.
- `DepartmentsController` (Admin) — CRUD departments + their service/price
  list.
- Extend SignalR: notify the target department's group when a referral
  arrives, and notify the referring doctor when a result is attached.

### Frontend
- "Refer patient" modal, launched from the encounter screen — pick a doctor
  or a department + service, add a reason.
- Department Portal screen — one generic component driven by department
  type/name, Kanban queue + a result-entry form (notes + file upload).

---

## Module D — Payment method, insurance, billing

Directly answers ask #1.

```
InsuranceProvider
  - Id, Name, ContactInfo, IsActive

PatientInsurance
  - Id, PatientId, InsuranceProviderId, PolicyNumber, CoveragePercentage,
    ValidFrom, ValidTo, IsActive

Invoice
  - Id, SourceAppointmentId, PatientId
  - Status (enum: Open, Settled, Cancelled)
  - PaymentMethod (enum: Cash, Insurance), PatientInsuranceId?
  - Subtotal, InsuranceCoveredAmount, PatientPayableAmount
  - CreatedAt, SettledAt?

InvoiceItem
  - Id, InvoiceId, Description, Amount
  - SourceType (enum: Consultation, Referral, Other), SourceReferralId?

Payment
  - Id, InvoiceId, Amount, Method (enum: Cash, Card, InsuranceClaim)
  - PaidAt, ReceivedByUserId
```

### Flow
1. At booking, Staff picks `PaymentMethod` — Cash, or Insurance (then picks
   which of the patient's active `PatientInsurance` records applies). This
   auto-creates an `Invoice` with a consultation-fee `InvoiceItem`.
2. Each completed `Referral` to a billable department appends an
   `InvoiceItem` (pulled from that department's `DepartmentService.Fee`) to
   the same open invoice.
3. At checkout, Staff settles the invoice. If Insurance,
   `CoveragePercentage` splits `Subtotal` into `InsuranceCoveredAmount` /
   `PatientPayableAmount`. `Payment` record(s) support partial payment over
   multiple transactions.

### Deferred to Phase C — full insurance claims lifecycle
Tracking whether the insurance company actually approved/paid the claim
(`InsuranceClaim`: ClaimAmount, ApprovedAmount, Status:
Submitted/Approved/Rejected/Paid) is real, but it's a second layer on top of
the basic coverage-% split above. Build the simple version first; add claims
tracking once the basic flow is proven out.

### Frontend
- Payment method + insurance picker at booking.
- Billing/Checkout screen — itemized invoice, payment recording, PDF export
  (reuse `IReportService`/QuestPDF).
- Admin: Insurance Providers management screen.
- Patient profile gains an "Insurance" tab to assign/manage policies.

---

## Module E — Production readiness (not requested, but needed for a real hospital)

None of this came from your list, but a system holding real patient and
payment data for an actual hospital needs it before go-live:

- **Soft delete** (`IsDeleted`, `DeletedAt`) on `Patient`, `MedicalRecord`,
  `Appointment`, `Invoice`, `Payment` — clinical and financial records should
  never be hard-deleted, only ever hidden/archived.
- **Audit log** — a new `AuditLog` entity (UserId, Action, EntityName,
  EntityId, Timestamp, Details) recording who changed what on
  `MedicalRecord`, `Payment`, `Referral`, and user/role changes. This matters
  both for dispute resolution ("who edited this diagnosis") and for basic
  accountability once multiple roles touch the same patient.
- **Concurrency tokens** (`RowVersion`) on `Invoice` and `Payment` — prevents
  two staff members from processing the same payment twice under concurrent
  access.
- **Secrets hygiene** — your current `CLAUDE.md` has the SQL Server
  connection string, including the `sa` password, committed in plain text in
  `appsettings.json`. Before this goes anywhere near production: move it to
  an environment variable / `dotnet user-secrets` / a secrets manager, and
  create a dedicated least-privilege database login for the app instead of
  using `sa`.
- **Backups** — you removed the WPF-era SQLite file-copy backup when
  migrating to SQL Server, which was correct at the time. But real
  patient/financial data now needs *some* backup story before production —
  likely scheduled SQL Server-native backups plus an offsite copy. This is
  more an infrastructure/ops decision than a code change, so decide the
  mechanism before building anything here.
- **Structured logging** (e.g., Serilog) — the existing file logger was fine
  for a single-user desktop app; a multi-role, multi-department web system
  benefits from structured, queryable logs.
- **Auth hardening** — rate limiting and lockout policy on login endpoints,
  since this is now reachable over a network rather than a trusted local
  session.
- **Data privacy — concrete for Egypt, not generic** — Egypt's Personal
  Data Protection Law (Law No. 151/2020) classifies health data as
  "sensitive personal data," which requires the patient's explicit consent
  to process, and which can require the hospital (as a data controller) to
  hold a separate sensitive-data license from the Personal Data Protection
  Center. The law's Executive Regulations were issued in November 2025 and
  gave a one-year grace period — full enforcement lands **31 October 2026**,
  which is inside the build window for this project, not a someday concern.
  Separately, Egypt's Penal Code, the Blood Operations Law (No. 8/2021), and
  the Mental Health Law (No. 71/2009) already impose their own
  confidentiality duties on health data specifically. None of this is legal
  advice — whether the hospital needs the sensitive-data license, a
  registered Data Protection Officer, or specific consent wording depends on
  facts about the hospital I don't have — but it's concrete enough to take
  to whoever handles the hospital's compliance before launch, rather than a
  vague "check local regulations" note.

---

## Module F — What real hospital ERPs also include (found by looking, not just asked)

You asked me not to rely only on your list, so I looked at how actual
hospital ERP/HMS products (general healthcare ERP vendors, hospital-specific
HMS platforms, and healthcare-specific CMMS/asset-management vendors) are
structured, and checked that against Modules A–E. Splitting what I found
into two buckets, because not all of it belongs in an *outpatient*
department manager specifically:

### Fits your current scope — worth adding alongside Phase C
These extend what's already being built without changing what kind of
system this is (outpatient, day-based, no admissions):

- **Staff duty roster.** `DoctorSchedule` already covers when a doctor is
  available for bookings, but real hospital software separately tracks
  which `Staff` and `DepartmentStaff` are actually rostered on a given shift
  (for coverage planning, not booking). A simple `StaffShift` entity
  (`UserId`, `ShiftDate`, `StartTime`, `EndTime`) is enough — nowhere near a
  full workforce-management product, just enough for Admin to see who's
  supposed to be on duty.
- **Lightweight equipment tracking.** Every hospital HMS I looked at
  includes some form of asset/equipment register tied to maintenance and
  compliance audits — full CMMS platforms exist as their own product
  category for this. You don't need that depth, but an `Asset` entity
  (`Name`, `DepartmentId`, `SerialNumber`, `PurchaseDate`, `WarrantyExpiry`,
  `LastMaintenanceDate`, `NextMaintenanceDue`) tied to `Department` is cheap
  to add while `Department` is already being built, and lab/radiology
  equipment failing without warning is a real operational risk.
- **Consent capture at registration** — see Module B and the Egypt-specific
  note above in Module E.

### Bigger scope — confirm before building, since these change what kind of system this is
Every real HMS I looked at treats the items below as core modules — but
they're core *because those products assume the hospital admits patients
and performs procedures*. This system is explicitly outpatient/day-visit
with no time-slot booking, so building any of these would be a genuine
scope expansion, not an extension of what's already planned:

- **Blood Bank** — donor management, blood-unit inventory with expiry
  tracking, cross-matching, and issuing units to departments like OT or
  Emergency. Only relevant if this hospital ever performs transfusions.
- **Operation Theatre (OT) management** — surgery scheduling, pre-op
  checklists and consent, anesthesia records, sterilization/instrument
  tracking, OT-specific billing. Only relevant if the hospital does surgery
  or day-procedures beyond an outpatient consultation.
- **Admission-Transfer-Discharge (ADT) / bed management** — ward
  assignment, bed occupancy, inpatient stay tracking. Only relevant if
  patients are ever kept overnight. This is the single biggest pivot on
  this list — the whole appointment model already built in this system
  (day-based, no time slots, queue order) assumes patients arrive, get
  seen, and leave the same day.
- **Full general ledger / accounting.** The `Invoice`/`Payment` design in
  Module D covers patient billing (revenue cycle) specifically — hospital-
  wide accounting (payroll, vendor payments, budgeting) is usually handled
  by dedicated finance software instead of built into the clinical system.
  Worth deciding whether to integrate with something like that later rather
  than building a general ledger here.

None of these four are built into Modules A–E above. Flagging them so
including or excluding them is a decision you make, not a default I picked
for you.

---

## Sequencing suggestion

Trying to build all of this in one pass is how projects stall. Suggested
order:

1. **Phase A — core clinical loop.** Doctor accounts/scoping (Module A),
   `MedicalRecord`/`Prescription`/`Attachment` (Module B), `Department` +
   `Referral` + `DepartmentQueueEntry` + department portal (Module C). At
   the end of this phase, a doctor can see a patient, chart the visit, and
   refer them to another doctor or department — no billing yet.
2. **Phase B — financial loop.** Payment method at booking, insurance
   providers/policies, `Invoice`/`InvoiceItem`/`Payment`, checkout screen
   (Module D, minus claims lifecycle).
3. **Phase C — production hardening.** Insurance claims lifecycle, audit
   log, soft deletes, concurrency tokens, secrets hygiene, backups,
   structured logging, auth hardening (Module E), plus the Module F items
   that fit current scope: staff duty roster, lightweight equipment
   tracking, consent capture.
4. **Phase D — optional/future, do not build unless explicitly requested:**
   pharmacy inventory/stock management, a patient-facing self-service
   portal, SMS/email appointment reminders, multi-branch/multi-hospital
   support. These are real hospital ERP features but each is close to its
   own project — flagging them here so they're a conscious future decision,
   not something built speculatively now.
5. **Phase E — only if the hospital's scope is bigger than outpatient:**
   Blood Bank, OT management, ADT/bed management, full accounting/GL (the
   "bigger scope" half of Module F). Do not start any of these without an
   explicit yes — each one is close to its own project, and ADT/bed
   management in particular changes the core assumptions the rest of this
   system is built on.

---

## Final step — rewrite CLAUDE.md again

Once implemented, rewrite `CLAUDE.md` in the same structure/detail as its
current version, updated to include at minimum:

- The 4 roles (Admin/Staff/Doctor/DepartmentStaff) and what each can access.
- All new entities from Modules B, C, D, F (and E if built): `MedicalRecord`,
  `Prescription`, `Attachment`, `Department`, `DepartmentService`,
  `Referral`, `DepartmentQueueEntry`, `InsuranceProvider`,
  `PatientInsurance`, `Invoice`, `InvoiceItem`, `Payment`, `StaffShift`,
  `Asset`, `ConsentRecord` (or the simpler `Patient.ConsentGivenAt` field),
  (`InsuranceClaim`, `AuditLog` if built; Blood Bank/OT/ADT entities only if
  Phase E was explicitly greenlit).
- New/updated `Patient` fields (`NationalId`, `Gender`, `KnownAllergies`).
- New controllers and their endpoints.
- New SignalR notifications (referral created/completed).
- New screens: Doctor Portal, Department Portal, Billing/Checkout, Insurance
  Providers admin, Patient Insurance tab, Patient Chart.
- Updated business rules section covering the referral flow and the
  billing/invoice flow end to end (mirroring how the current file documents
  the Queue/Status Sync table — a similar before/after table for referrals
  and invoices would keep the same style).
- An updated "What Has Been Completed" / "What Still Needs To Be Done" —
  reflecting actual state once you've done the work, phase by phase, not a
  flat list.

Treat it as a full rewrite once each phase lands, same as last time — future
sessions should be able to work from `CLAUDE.md` alone.

---

## Worth confirming before starting

A few of the decisions above are policy calls, not technical ones —
flagging them rather than guessing:

- **Cross-doctor record access** — should any doctor be able to read any
  patient's full history (continuity of care), or only patients they've
  personally treated?
- **Insurance coverage model** — one flat coverage % per policy (as
  designed above) is the simple version; real insurers often cover
  different percentages per service type (e.g., consultation vs. lab).
  Start simple and extend later, or model per-service coverage from the
  start?
- **Backup mechanism** — scheduled SQL Server Agent job, a script + cloud
  storage, or a managed SQL Server offering with backups built in? Depends
  partly on where this will be hosted, which you know and I don't.
- **Phase D features** — is pharmacy inventory or a patient self-service
  portal actually wanted at some point, or genuinely out of scope? Doesn't
  need an answer now, just worth knowing so it isn't quietly forgotten or
  quietly built.
- **Scope of care** — does this hospital ever admit patients overnight,
  perform surgery/procedures, or handle blood transfusions? This is the one
  that matters most from Module F — it decides whether Blood Bank, OT, and
  bed management ever belong on the roadmap, or stay permanently out of
  scope for what is, right now, a same-day outpatient system.
- **Staff duty roster** — worth adding now alongside Phase C, or skip for
  now since it's an operational convenience rather than a clinical or
  financial one?
