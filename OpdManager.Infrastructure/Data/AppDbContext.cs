using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpdManager.Domain.Entities;
using OpdManager.Infrastructure.Identity;

namespace OpdManager.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<QueueEntry> QueueEntries => Set<QueueEntry>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // ── Phase A: Clinical entities ──────────────────────────────────────────
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<DepartmentService> DepartmentServices => Set<DepartmentService>();
    public DbSet<Referral> Referrals => Set<Referral>();
    public DbSet<DepartmentQueueEntry> DepartmentQueueEntries => Set<DepartmentQueueEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Identity tables

        // Enum String Conversions
        modelBuilder.Entity<Appointment>().Property(e => e.Status).HasConversion<string>();
        modelBuilder.Entity<QueueEntry>().Property(e => e.Status).HasConversion<string>();
        modelBuilder.Entity<Issue>().Property(e => e.Status).HasConversion<string>();
        modelBuilder.Entity<Referral>().Property(e => e.Status).HasConversion<string>();
        modelBuilder.Entity<DepartmentQueueEntry>().Property(e => e.Status).HasConversion<string>();

        // Doctor
        modelBuilder.Entity<Doctor>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Name).IsRequired().HasMaxLength(100);
            e.Property(d => d.Specialty).IsRequired().HasMaxLength(100);
            e.HasIndex(d => d.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL");
        });

        // Patient
        modelBuilder.Entity<Patient>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.FullName).IsRequired().HasMaxLength(150);
            e.Property(p => p.NationalId).HasMaxLength(50);
            e.HasIndex(p => p.NationalId).IsUnique().HasFilter("[NationalId] IS NOT NULL");
        });

        // Appointment — no more TimeSlotTicks, day-based
        modelBuilder.Entity<Appointment>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasOne(a => a.Doctor)
             .WithMany(d => d.Appointments)
             .HasForeignKey(a => a.DoctorId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Patient)
             .WithMany(p => p.Appointments)
             .HasForeignKey(a => a.PatientId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // QueueEntry
        modelBuilder.Entity<QueueEntry>(e =>
        {
            e.HasKey(q => q.Id);
            e.Ignore(q => q.WaitingMinutes);
            e.Ignore(q => q.ExaminationMinutes);
            e.HasOne(q => q.Appointment)
             .WithOne(a => a.QueueEntry)
             .HasForeignKey<QueueEntry>(q => q.AppointmentId);
        });

        // Issue
        modelBuilder.Entity<Issue>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Title).IsRequired().HasMaxLength(200);
        });

        // DoctorSchedule — per-weekday schedule
        modelBuilder.Entity<DoctorSchedule>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.Doctor)
             .WithMany(d => d.Schedules)
             .HasForeignKey(s => s.DoctorId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(s => new { s.DoctorId, s.DayOfWeek }).IsUnique();
            // SQL Server maps TimeSpan to time(7) natively — no ticks workaround needed
        });

        // AppSetting — key-value store
        modelBuilder.Entity<AppSetting>(e =>
        {
            e.HasKey(s => s.Key);
            e.Property(s => s.Key).HasMaxLength(100);
            e.Property(s => s.Value).HasMaxLength(500);
        });

        // RefreshToken
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.Token).IsUnique();
            e.HasOne(t => t.User)
             .WithMany()
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ═══════════════════════════════════════════════════════════════════
        // Phase A: Clinical entities
        // ═══════════════════════════════════════════════════════════════════

        // MedicalRecord — one-to-one with Appointment
        modelBuilder.Entity<MedicalRecord>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasOne(r => r.Appointment)
             .WithOne(a => a.MedicalRecord)
             .HasForeignKey<MedicalRecord>(r => r.AppointmentId);
            e.HasOne(r => r.Doctor)
             .WithMany(d => d.MedicalRecords)
             .HasForeignKey(r => r.DoctorId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Patient)
             .WithMany(p => p.MedicalRecords)
             .HasForeignKey(r => r.PatientId)
             .OnDelete(DeleteBehavior.Restrict);
            e.Property(r => r.TemperatureC).HasPrecision(5, 2);
            e.Property(r => r.WeightKg).HasPrecision(6, 2);
            e.Property(r => r.HeightCm).HasPrecision(6, 2);
        });

        // Prescription
        modelBuilder.Entity<Prescription>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.MedicationName).IsRequired().HasMaxLength(200);
            e.HasOne(p => p.MedicalRecord)
             .WithMany(r => r.Prescriptions)
             .HasForeignKey(p => p.MedicalRecordId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Attachment
        modelBuilder.Entity<Attachment>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.FileName).IsRequired().HasMaxLength(255);
            e.Property(a => a.FilePath).IsRequired().HasMaxLength(500);
            e.HasOne(a => a.MedicalRecord)
             .WithMany(r => r.Attachments)
             .HasForeignKey(a => a.MedicalRecordId)
             .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(a => a.Referral)
             .WithMany(r => r.Attachments)
             .HasForeignKey(a => a.ReferralId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // Department
        modelBuilder.Entity<Department>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Name).IsRequired().HasMaxLength(100);
        });

        // DepartmentService
        modelBuilder.Entity<DepartmentService>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).IsRequired().HasMaxLength(200);
            e.Property(s => s.Fee).HasPrecision(18, 2);
            e.HasOne(s => s.Department)
             .WithMany(d => d.Services)
             .HasForeignKey(s => s.DepartmentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Referral
        modelBuilder.Entity<Referral>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasOne(r => r.Patient)
             .WithMany(p => p.Referrals)
             .HasForeignKey(r => r.PatientId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.ReferringDoctor)
             .WithMany(d => d.ReferralsCreated)
             .HasForeignKey(r => r.ReferringDoctorId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.SourceAppointment)
             .WithMany()
             .HasForeignKey(r => r.SourceAppointmentId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.TargetDoctor)
             .WithMany(d => d.ReferralsReceived)
             .HasForeignKey(r => r.TargetDoctorId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.TargetDepartment)
             .WithMany(d => d.Referrals)
             .HasForeignKey(r => r.TargetDepartmentId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.TargetDepartmentService)
             .WithMany()
             .HasForeignKey(r => r.TargetDepartmentServiceId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // DepartmentQueueEntry
        modelBuilder.Entity<DepartmentQueueEntry>(e =>
        {
            e.HasKey(q => q.Id);
            e.HasOne(q => q.Referral)
             .WithOne(r => r.DepartmentQueueEntry)
             .HasForeignKey<DepartmentQueueEntry>(q => q.ReferralId);
            e.HasOne(q => q.Department)
             .WithMany(d => d.QueueEntries)
             .HasForeignKey(q => q.DepartmentId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ApplicationUser — Doctor / Department links
        modelBuilder.Entity<ApplicationUser>(e =>
        {
            e.HasIndex(u => u.DoctorId).IsUnique().HasFilter("[DoctorId] IS NOT NULL");
            e.HasIndex(u => u.DepartmentId).HasFilter("[DepartmentId] IS NOT NULL");
        });
    }
}
