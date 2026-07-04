using OpdManager.Domain.Entities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace OpdManager.Presentation.Helpers;

public static class PrintHelper
{
    public static void PrintAppointments(List<Appointment> data)
    {
        var dlg = new PrintDialog();
        if (dlg.ShowDialog() != true) return;

        var pageSize = new Size(dlg.PrintableAreaWidth, dlg.PrintableAreaHeight);

        var doc = new FixedDocument();
        var page = new FixedPage { Width = pageSize.Width, Height = pageSize.Height };
        var canvas = new StackPanel { Margin = new Thickness(40) };

        // Title
        canvas.Children.Add(new TextBlock
        {
            Text = "OPD Manager — Appointments Report",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(28, 40, 51)),
            Margin = new Thickness(0, 0, 0, 4)
        });

        canvas.Children.Add(new TextBlock
        {
            Text = $"Generated: {DateTime.Now:dddd, MMMM dd, yyyy h:mm tt}",
            FontSize = 11,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 0, 0, 20)
        });

        // Header row
        canvas.Children.Add(BuildRow(
            new[] { "Queue #", "Patient", "Doctor", "Time", "Status", "Notes" },
            isHeader: true,
            pageSize.Width - 80));

        // Data rows
        for (int i = 0; i < data.Count; i++)
        {
            var a = data[i];
            canvas.Children.Add(BuildRow(
                new[]
                {
                    a.QueueEntry?.QueueNumber.ToString() ?? "-",
                    a.Patient?.FullName  ?? "-",
                    a.Doctor?.Name       ?? "-",
                    DateTime.Today.Add(a.TimeSlot).ToString("h:mm tt"),
                    a.Status.ToString(),
                    a.Notes              ?? "-"
                },
                isHeader: false,
                pageSize.Width - 80,
                i));
        }

        page.Children.Add(canvas);

        var pageContent = new PageContent();
        ((System.Windows.Markup.IAddChild)pageContent).AddChild(page);
        doc.Pages.Add(pageContent);

        dlg.PrintDocument(doc.DocumentPaginator, "OPD Appointments Report");
    }

    private static Grid BuildRow(string[] values, bool isHeader, double totalWidth, int rowIndex = 0)
    {
        var grid = new Grid
        {
            Width = totalWidth,
            Background = isHeader
                ? new SolidColorBrush(Color.FromRgb(28, 40, 51))
                : rowIndex % 2 == 0
                    ? Brushes.White
                    : new SolidColorBrush(Color.FromRgb(245, 245, 245))
        };

        // Equal columns
        for (int i = 0; i < values.Length; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        for (int i = 0; i < values.Length; i++)
        {
            var tb = new TextBlock
            {
                Text = values[i],
                Padding = new Thickness(8, 6, 8, 6),
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                Foreground = isHeader ? Brushes.White : Brushes.Black,
                FontSize = 11,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            Grid.SetColumn(tb, i);
            grid.Children.Add(tb);
        }

        // Bottom border
        grid.Children.Add(new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Height = 1,
            VerticalAlignment = VerticalAlignment.Bottom
        });

        return grid;
    }
}