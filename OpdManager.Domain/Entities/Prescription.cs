namespace OpdManager.Domain.Entities;

public class Prescription
{
    public int Id { get; set; }
    public int MedicalRecordId { get; set; }

    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public int? DurationDays { get; set; }
    public string? Instructions { get; set; }

    // Navigation
    public MedicalRecord MedicalRecord { get; set; } = null!;
}
