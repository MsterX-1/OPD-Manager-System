using OpdManager.Domain.Enums;

namespace OpdManager.Domain.Entities;

/// <summary>
/// Represents an internal clinic, lab, or radiology unit within the hospital.
/// Services are offered by departments and patients are referred here from primary doctors.
/// </summary>
public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DepartmentType Type { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<DepartmentService> Services { get; set; } = new List<DepartmentService>();
    public ICollection<DepartmentQueueEntry> QueueEntries { get; set; } = new List<DepartmentQueueEntry>();
    public ICollection<Referral> Referrals { get; set; } = new List<Referral>();
}
