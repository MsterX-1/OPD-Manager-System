namespace OpdManager.Domain.Entities;

public class Attachment
{
    public int Id { get; set; }

    /// <summary>
    /// Nullable — attached to a medical record (e.g., doctor uploads a scan).
    /// </summary>
    public int? MedicalRecordId { get; set; }

    /// <summary>
    /// Nullable — attached to a referral result (e.g., lab uploads results).
    /// </summary>
    public int? ReferralId { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public string? UploadedByUserId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public MedicalRecord? MedicalRecord { get; set; }
    public Referral? Referral { get; set; }
}
