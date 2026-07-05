using Microsoft.AspNetCore.Identity;

namespace OpdManager.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Links this login to a Doctor record. Nullable — only set for users with the "Doctor" role.
    /// </summary>
    public int? DoctorId { get; set; }

    /// <summary>
    /// Links this login to a Department. Nullable — only set for users with the "DepartmentStaff" role.
    /// </summary>
    public int? DepartmentId { get; set; }
}
