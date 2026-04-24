using OpdManager.Domain.Enums;
using System.Globalization;
using System.Windows.Data;

namespace OpdManager.Presentation.Converters;

public class StatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is AppointmentStatus status ? status switch
        {
            AppointmentStatus.Scheduled => "● Scheduled",
            AppointmentStatus.InProgress => "● In Progress",
            AppointmentStatus.Completed => "● Completed",
            AppointmentStatus.Cancelled => "● Cancelled",
            AppointmentStatus.NoShow => "● No Show",
            _ => "● Unknown"
        } : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}