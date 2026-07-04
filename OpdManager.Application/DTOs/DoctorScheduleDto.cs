namespace OpdManager.Application.DTOs;

public class DoctorScheduleDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsWorking { get; set; }
}
