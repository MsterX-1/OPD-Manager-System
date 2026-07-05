namespace OpdManager.Domain.Entities;

public class DepartmentService
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }

    /// <summary>
    /// Service name, e.g., "CBC", "Chest X-Ray", "Blood Sugar Test".
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Service fee — used by billing in Phase B.
    /// </summary>
    public decimal Fee { get; set; }

    // Navigation
    public Department Department { get; set; } = null!;
}
