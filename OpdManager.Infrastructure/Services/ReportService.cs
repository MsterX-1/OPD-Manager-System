using ClosedXML.Excel;
using OpdManager.Application.DTOs;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OpdManager.Infrastructure.Services;

public class ReportService : IReportService
{
    public ReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] ExportAppointmentsToPdf(List<Appointment> data, string hospitalName)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(c => ComposeAppointmentHeader(c, hospitalName));
                page.Content().Element(c => ComposeAppointmentContent(c, data));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    public byte[] ExportAppointmentsToExcel(List<Appointment> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Appointments");

        var headers = new[] { "Queue #", "Patient", "Doctor", "Specialty", "Date", "Status", "Notes" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1C2833");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        for (int i = 0; i < data.Count; i++)
        {
            var a = data[i];
            var row = i + 2;

            worksheet.Cell(row, 1).Value = a.QueueEntry?.QueueNumber.ToString() ?? "-";
            worksheet.Cell(row, 2).Value = a.Patient?.FullName ?? "-";
            worksheet.Cell(row, 3).Value = a.Doctor?.Name ?? "-";
            worksheet.Cell(row, 4).Value = a.Doctor?.Specialty ?? "-";
            worksheet.Cell(row, 5).Value = a.AppointmentDate.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 6).Value = a.Status.ToString();
            worksheet.Cell(row, 7).Value = a.Notes ?? "-";

            if (i % 2 == 0)
                worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");
        }

        worksheet.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportDoctorPerformanceToPdf(
        List<DoctorPerformanceDto> data, DateTime from, DateTime to, string hospitalName)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"{hospitalName} — Doctor Performance Report")
                           .FontSize(18).SemiBold().FontColor(Colors.Blue.Darken3);
                        col.Item().Text($"Period: {from:MMM dd, yyyy} – {to:MMM dd, yyyy}")
                           .FontSize(11).FontColor(Colors.Grey.Medium);
                        col.Item().Text($"Generated: {DateTime.Now:dddd, MMMM dd, yyyy h:mm tt}")
                           .FontSize(11).FontColor(Colors.Grey.Medium);
                    });
                });

                page.Content().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(30);  // #
                        cols.RelativeColumn(2);   // Doctor
                        cols.RelativeColumn(1);   // Patients
                        cols.RelativeColumn(1.5f);// Avg Min
                        cols.RelativeColumn(1.5f);// Total Min
                    });

                    table.Header(header =>
                    {
                        foreach (var h in new[] { "#", "Doctor", "Patients", "Avg (min)", "Total (min)" })
                        {
                            header.Cell()
                                  .Background(Colors.Blue.Darken3)
                                  .Padding(8)
                                  .Text(h)
                                  .FontColor(Colors.White)
                                  .SemiBold();
                        }
                    });

                    for (int i = 0; i < data.Count; i++)
                    {
                        var d = data[i];
                        var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        table.Cell().Background(bg).Padding(8).Text((i + 1).ToString());
                        table.Cell().Background(bg).Padding(8).Text(d.DoctorName);
                        table.Cell().Background(bg).Padding(8).Text(d.PatientsExamined.ToString());
                        table.Cell().Background(bg).Padding(8).Text(d.AverageExaminationMinutes?.ToString("F1") ?? "-");
                        table.Cell().Background(bg).Padding(8).Text(d.TotalExaminationMinutes.ToString("F1"));
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    public byte[] ExportDoctorPerformanceToExcel(
        List<DoctorPerformanceDto> data, DateTime from, DateTime to)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Doctor Performance");

        var headers = new[] { "#", "Doctor", "Patients Examined", "Avg Examination (min)", "Total Examination (min)" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1C2833");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        for (int i = 0; i < data.Count; i++)
        {
            var d = data[i];
            var row = i + 2;

            worksheet.Cell(row, 1).Value = i + 1;
            worksheet.Cell(row, 2).Value = d.DoctorName;
            worksheet.Cell(row, 3).Value = d.PatientsExamined;
            worksheet.Cell(row, 4).Value = d.AverageExaminationMinutes?.ToString("F1") ?? "-";
            worksheet.Cell(row, 5).Value = d.TotalExaminationMinutes.ToString("F1");

            if (i % 2 == 0)
                worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");
        }

        worksheet.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private static void ComposeAppointmentHeader(IContainer container, string hospitalName)
    {
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

    private static void ComposeAppointmentContent(IContainer container, List<Appointment> data)
    {
        container.PaddingTop(20).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(30);  // #
                cols.RelativeColumn(2);   // Patient
                cols.RelativeColumn(2);   // Doctor
                cols.RelativeColumn(1);   // Date
                cols.RelativeColumn(1);   // Status
            });

            table.Header(header =>
            {
                foreach (var h in new[] { "#", "Patient", "Doctor", "Date", "Status" })
                {
                    header.Cell()
                          .Background(Colors.Blue.Darken3)
                          .Padding(8)
                          .Text(h)
                          .FontColor(Colors.White)
                          .SemiBold();
                }
            });

            for (int i = 0; i < data.Count; i++)
            {
                var a = data[i];
                var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                table.Cell().Background(bg).Padding(8).Text((i + 1).ToString());
                table.Cell().Background(bg).Padding(8).Text(a.Patient?.FullName ?? "-");
                table.Cell().Background(bg).Padding(8).Text(a.Doctor?.Name ?? "-");
                table.Cell().Background(bg).Padding(8).Text(a.AppointmentDate.ToString("yyyy-MM-dd"));
                table.Cell().Background(bg).Padding(8).Text(a.Status.ToString());
            }
        });
    }
}