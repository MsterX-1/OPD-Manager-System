using OpdManager.Domain.Enums;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace OpdManager.Presentation.Converters;

public class QueueStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is QueueStatus status ? status switch
        {
            QueueStatus.Waiting => new SolidColorBrush(Color.FromRgb(230, 81, 0)),
            QueueStatus.InProgress => new SolidColorBrush(Color.FromRgb(21, 101, 192)),
            QueueStatus.Done => new SolidColorBrush(Color.FromRgb(46, 125, 50)),
            QueueStatus.Skipped => new SolidColorBrush(Color.FromRgb(120, 144, 156)),
            _ => new SolidColorBrush(Colors.Gray)
        } : new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}