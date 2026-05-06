using AutoServiceBooking.Web.Extensions;
using AutoServiceBooking.Web.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AutoServiceBooking.Web.Services.Exports
{
    public class ExportService : IExportService
    {
        public byte[] CreateBookingActPdf(Booking booking)
        {
            DateTime scheduledAt = booking.ScheduledAt.ToLocalTime();
            DateTime? completedAt = booking.CompletedAt?.ToLocalTime();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(text => text.FontFamily("Arial").FontSize(11));

                    page.Header().Column(column =>
                    {
                        column.Item().Text("DriveFix Service").FontSize(22).Bold().FontColor(Colors.Blue.Darken3);
                        column.Item().Text($"Акт виконаних робіт №{booking.Id}").FontSize(16).Bold();
                        column.Item().Text($"Сформовано: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingVertical(20).Column(column =>
                    {
                        column.Spacing(16);

                        column.Item().Text("Інформація про клієнта").FontSize(13).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(130);
                                columns.RelativeColumn();
                            });

                            AddInfoRow(table, "Клієнт", booking.CustomerName);
                            AddInfoRow(table, "Телефон", booking.CustomerPhone);
                            AddInfoRow(table, "Email", booking.CustomerEmail ?? "Не вказано");
                        });

                        column.Item().Text("Автомобіль та послуга").FontSize(13).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(130);
                                columns.RelativeColumn();
                            });

                            AddInfoRow(table, "Авто", GetVehicleTitle(booking));
                            AddInfoRow(table, "Послуга", booking.AutoService.Name);
                            AddInfoRow(table, "Дата запису", scheduledAt.ToString("dd.MM.yyyy HH:mm"));
                            AddInfoRow(table, "Дата завершення", completedAt?.ToString("dd.MM.yyyy HH:mm") ?? "Не вказано");
                            AddInfoRow(table, "Статус", booking.Status.GetDisplayName());
                        });

                        column.Item().Text("Фінанси").FontSize(13).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(130);
                                columns.RelativeColumn();
                            });

                            AddInfoRow(table, "Фінальна ціна", FormatMoney(booking.FinalPrice));
                        });

                        if (!string.IsNullOrWhiteSpace(booking.ProblemDescription))
                        {
                            column.Item().Text("Опис клієнта").FontSize(13).Bold();
                            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Text(booking.ProblemDescription);
                        }

                        if (!string.IsNullOrWhiteSpace(booking.AdminComment))
                        {
                            column.Item().Text("Коментар майстра").FontSize(13).Bold();
                            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Text(booking.AdminComment);
                        }

                        column.Item().PaddingTop(20).Row(row =>
                        {
                            row.RelativeItem().Text("Підпис клієнта: ____________________");
                            row.RelativeItem().AlignRight().Text("Підпис сервісу: ____________________");
                        });
                    });

                    page.Footer().AlignCenter().DefaultTextStyle(textStyle => textStyle.FontSize(9).FontColor(Colors.Grey.Darken1)).Text(text =>
                    {
                        text.Span("DriveFix Service · ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        public byte[] CreateVehicleHistoryPdf(Vehicle vehicle, IReadOnlyList<Booking> bookings)
        {
            List<Booking> completedBookings = bookings
                .Where(booking => booking.Status == BookingStatus.Completed)
                .OrderByDescending(booking => booking.ScheduledAt)
                .ToList();
            decimal totalSpent = completedBookings.Sum(booking => booking.FinalPrice ?? 0);
            DateTime? lastServiceAt = completedBookings.FirstOrDefault()?.ScheduledAt.ToLocalTime();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(text => text.FontFamily("Arial").FontSize(10));

                    page.Header().Column(column =>
                    {
                        column.Item().Text("DriveFix Service").FontSize(22).Bold().FontColor(Colors.Blue.Darken3);
                        column.Item().Text("Сервісна історія автомобіля").FontSize(16).Bold();
                        column.Item().Text($"Сформовано: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingVertical(20).Column(column =>
                    {
                        column.Spacing(16);

                        column.Item().Text($"{vehicle.Make} {vehicle.Model} ({vehicle.Year})").FontSize(15).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(130);
                                columns.RelativeColumn();
                            });

                            AddInfoRow(table, "Номерний знак", vehicle.LicensePlate);
                            AddInfoRow(table, "Пробіг", $"{vehicle.Mileage} км");
                            AddInfoRow(table, "Тип пального", vehicle.FuelType.GetDisplayName());
                            AddInfoRow(table, "Стан", vehicle.IsArchived ? "В архіві" : "Активний");
                        });

                        column.Item().Text("Підсумок").FontSize(13).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            AddSummaryCell(table, "Виконано робіт", completedBookings.Count.ToString());
                            AddSummaryCell(table, "Останній сервіс", lastServiceAt?.ToString("dd.MM.yyyy") ?? "Немає");
                            AddSummaryCell(table, "Витрати", FormatMoney(totalSpent));
                        });

                        column.Item().Text("Виконані роботи").FontSize(13).Bold();

                        if (completedBookings.Count == 0)
                        {
                            column.Item().Text("Для цього автомобіля ще немає завершених сервісних робіт.").FontColor(Colors.Grey.Darken1);
                        }
                        else
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(75);
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(85);
                                    columns.ConstantColumn(75);
                                });

                                AddTableHeader(table, "Дата");
                                AddTableHeader(table, "Послуга");
                                AddTableHeader(table, "Статус");
                                AddTableHeader(table, "Вартість");

                                foreach (Booking booking in completedBookings)
                                {
                                    AddTableCell(table, booking.ScheduledAt.ToLocalTime().ToString("dd.MM.yyyy"));
                                    AddTableCell(table, $"#{booking.Id} · {booking.AutoService.Name}");
                                    AddTableCell(table, booking.Status.GetDisplayName());
                                    AddTableCell(table, FormatMoney(booking.FinalPrice));
                                }
                            });
                        }

                        column.Item().Text("Документ можна використовувати як коротке підтвердження сервісної історії автомобіля.")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    page.Footer().AlignCenter().DefaultTextStyle(textStyle => textStyle.FontSize(9).FontColor(Colors.Grey.Darken1)).Text(text =>
                    {
                        text.Span("DriveFix Service · ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        private static void AddInfoRow(TableDescriptor table, string label, string value)
        {
            table.Cell().Element(InfoLabelCell).Text(label).SemiBold();
            table.Cell().Element(InfoValueCell).Text(value);
        }

        private static IContainer InfoLabelCell(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingRight(8);
        }

        private static IContainer InfoValueCell(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
        }

        private static void AddSummaryCell(TableDescriptor table, string label, string value)
        {
            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(column =>
            {
                column.Item().Text(label).FontSize(9).FontColor(Colors.Grey.Darken1);
                column.Item().Text(value).FontSize(13).Bold();
            });
        }

        private static void AddTableHeader(TableDescriptor table, string value)
        {
            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(value).SemiBold();
        }

        private static void AddTableCell(TableDescriptor table, string value)
        {
            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(value);
        }

        private static string FormatMoney(decimal? value)
        {
            return value.HasValue ? $"{value.Value:0.##} грн" : "Не вказано";
        }

        private static string GetVehicleTitle(Booking booking)
        {
            if (booking.Vehicle != null)
            {
                return $"{booking.Vehicle.LicensePlate} — {booking.Vehicle.Make} {booking.Vehicle.Model} ({booking.Vehicle.Year})";
            }

            return $"{booking.GuestVehicleLicensePlate} — {booking.GuestVehicleMake} {booking.GuestVehicleModel} ({booking.GuestVehicleYear})";
        }
    }
}
