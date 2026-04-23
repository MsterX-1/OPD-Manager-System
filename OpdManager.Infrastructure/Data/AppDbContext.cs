using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OpdManager.Domain.Entities;

namespace OpdManager.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Issue> Issues => Set<Issue>();
        public DbSet<QueueEntry> QueueEntries => Set<QueueEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

            // Appointment
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
                e.Ignore(q => q.WaitingMinutes); // computed, not stored
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
        }
    }
}
