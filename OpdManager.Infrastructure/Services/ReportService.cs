using ClosedXML.Excel;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Diagnostics;

namespace OpdManager.Infrastructure.Services;

public class ReportService : IReportService
{
    public ReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public void ExportAppointmentsToPdf(List<Appointment> data, string filePath)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(ComposeHeader);
                page.Content().Element(c => ComposeContent(c, data));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf(filePath);

        // Open the file after export
        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
    }

    private static void ComposeHeader(IContainer container)
    {
        var hospitalName = "OPD Manager"; // default
        try {
            // Read settings.json directly (Infrastructure can't reference Presentation)
            var settingsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            if (System.IO.File.Exists(settingsPath))
            {
                var json = System.IO.File.ReadAllText(settingsPath);
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("HospitalName", out var prop))
                    hospitalName = prop.GetString() ?? hospitalName;
            }
        } catch { }

        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text($"{hospitalName} — Appointments Report")
                   .FontSize(18).SemiBold().FontColor(Colors.Blue.Darken3);
                col.Item().Text($"Generated: {DateTime.Now:dddd, MMMM dd, yyyy h:mm tt}")
                   .FontSize(11).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private static void ComposeContent(IContainer container, List<Appointment> data)
    {
        container.PaddingTop(20).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(30);  // #
                cols.RelativeColumn(2);   // Patient
                cols.RelativeColumn(2);   // Doctor
                cols.RelativeColumn(1);   // Time
                cols.RelativeColumn(1);   // Status
            });

            // Header row
            table.Header(header =>
            {
                foreach (var h in new[] { "#", "Patient", "Doctor", "Time", "Status" })
                {
                    header.Cell()
                          .Background(Colors.Blue.Darken3)
                          .Padding(8)
                          .Text(h)
                          .FontColor(Colors.White)
                          .SemiBold();
                }
            });

            // Data rows
            for (int i = 0; i < data.Count; i++)
            {
                var a = data[i];
                var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                table.Cell().Background(bg).Padding(8).Text((i + 1).ToString());
                table.Cell().Background(bg).Padding(8).Text(a.Patient?.FullName ?? "-");
                table.Cell().Background(bg).Padding(8).Text(a.Doctor?.Name ?? "-");
                table.Cell().Background(bg).Padding(8)
                     .Text(DateTime.Today.Add(a.TimeSlot).ToString("h:mm tt"));
                table.Cell().Background(bg).Padding(8).Text(a.Status.ToString());
            }
        });
    }

    public void ExportAppointmentsToExcel(List<Appointment> data, string filePath)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Appointments");

        // Headers
        var headers = new[] { "Queue #", "Patient", "Doctor", "Specialty", "Time", "Status", "Notes" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1C2833");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Data rows
        for (int i = 0; i < data.Count; i++)
        {
            var a = data[i];
            var row = i + 2;

            worksheet.Cell(row, 1).Value = a.QueueEntry?.QueueNumber.ToString() ?? "-";
            worksheet.Cell(row, 2).Value = a.Patient?.FullName ?? "-";
            worksheet.Cell(row, 3).Value = a.Doctor?.Name ?? "-";
            worksheet.Cell(row, 4).Value = a.Doctor?.Specialty ?? "-";
            worksheet.Cell(row, 5).Value = DateTime.Today.Add(a.TimeSlot).ToString("h:mm tt");
            worksheet.Cell(row, 6).Value = a.Status.ToString();
            worksheet.Cell(row, 7).Value = a.Notes ?? "-";

            if (i % 2 == 0)
                worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");
        }

        worksheet.Columns().AdjustToContents();
        workbook.SaveAs(filePath);

        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
    }
}