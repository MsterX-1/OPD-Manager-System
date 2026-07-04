namespace OpdManager.Application.DTOs;

public class AppSettingsDto
{
    public string HospitalName { get; set; } = "OPD Manager";
    public int WaitingAlertMinutes { get; set; } = 30;
    public bool EnableDailyAppointmentCap { get; set; } = false;
    public int DefaultAverageExaminationMinutes { get; set; } = 15;
}
