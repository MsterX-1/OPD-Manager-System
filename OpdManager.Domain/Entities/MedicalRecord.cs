namespace OpdManager.Domain.Entities;

/// <summary>
/// A persistent clinical document created during an encounter.
/// Stores vitals, diagnoses, and serves as the parent container for prescriptions.
/// </summary>
public class MedicalRecord
{
    public int Id { get; set; }

    /// <summary>
    /// One-to-one with Appointment — one visit, one record.
    /// </summary>
    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }

    // ── Vitals (editable by Staff or Doctor) ────────────────────────────────
    public int? BloodPressureSystolic { get; set; }
    public int? BloodPressureDiastolic { get; set; }
    public decimal? TemperatureC { get; set; }
    public int? PulseRate { get; set; }
    public int? RespiratoryRate { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public int? OxygenSaturation { get; set; }

    // ── Clinical fields (Doctor-only) ───────────────────────────────────────
    public string? ChiefComplaint { get; set; }
    public string? Diagnosis { get; set; }
    public string? ClinicalNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Appointment Appointment { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
