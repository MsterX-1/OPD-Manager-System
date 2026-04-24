using OpdManager.Domain.Enums;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace OpdManager.Presentation.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is AppointmentStatus status ? status switch
        {
            AppointmentStatus.Scheduled => new SolidColorBrush(Color.FromRgb(21, 101, 192)),
            AppointmentStatus.InProgress => new SolidColorBrush(Color.FromRgb(230, 81, 0)),
            AppointmentStatus.Completed => new SolidColorBrush(Color.FromRgb(46, 125, 50)),
            AppointmentStatus.Cancelled => new SolidColorBrush(Color.FromRgb(198, 40, 40)),
            AppointmentStatus.NoShow => new SolidColorBrush(Color.FromRgb(120, 144, 156)),
            _ => new SolidColorBrush(Colors.Gray)
        } : new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}