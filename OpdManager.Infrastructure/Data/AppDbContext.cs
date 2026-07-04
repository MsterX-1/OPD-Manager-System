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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Identity tables

        // Doctor
        modelBuilder.Entity<Doctor>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Name).IsRequired().HasMaxLength(100);
            e.Property(d => d.Specialty).IsRequired().HasMaxLength(100);
        });

        // Patient
        modelBuilder.Entity<Patient>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.FullName).IsRequired().HasMaxLength(150);
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
    }
}
