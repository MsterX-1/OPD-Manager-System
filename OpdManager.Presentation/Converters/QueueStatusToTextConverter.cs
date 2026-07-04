using OpdManager.Domain.Enums;
using System.Globalization;
using System.Windows.Data;

namespace OpdManager.Presentation.Converters;

public class QueueStatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is QueueStatus status ? status switch
        {
            QueueStatus.Waiting => "⏳ Waiting",
            QueueStatus.InProgress => "🔄 In Progress",
            QueueStatus.Done => "✅ Done",
            QueueStatus.Skipped => "⏭ Skipped",
            _ => "Unknown"
        } : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}