namespace OpdManager.Application.DTOs;

public class DoctorPerformanceDto
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int PatientsExamined { get; set; }
    public double? AverageExaminationMinutes { get; set; }
    public double TotalExaminationMinutes { get; set; }
    public double? AverageWaitingMinutes { get; set; }
    public double TotalWaitingMinutes { get; set; }
}
