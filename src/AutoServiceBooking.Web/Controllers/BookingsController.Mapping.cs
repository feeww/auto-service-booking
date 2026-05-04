using AutoServiceBooking.Web.Extensions;
using AutoServiceBooking.Web.Models;
using AutoServiceBooking.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBooking.Web.Controllers
{
    public partial class BookingsController
    {
        private const int BookingPageSize = 10;

        private BookingListPageViewModel CreateBookingListPage(
            List<Booking> bookings,
            string? search,
            BookingStatus? status,
            string sort,
            bool isAdminView,
            int page,
            int totalItems)
        {
            string selectedSort = NormalizeSort(sort);

            return new BookingListPageViewModel
            {
                Bookings = bookings
                    .Select(booking => CreateBookingListItem(booking, isAdminView))
                    .ToList(),
                Search = search,
                Status = status,
                Sort = selectedSort,
                Page = page,
                PageSize = BookingPageSize,
                TotalItems = totalItems,
                TotalPages = CalculateTotalPages(totalItems),
                StatusOptions = Enum.GetValues<BookingStatus>()
                    .Select(currentStatus => new BookingStatusOptionViewModel
                    {
                        Value = currentStatus.ToString(),
                        Text = currentStatus.GetDisplayName()
                    })
                    .ToList(),
                SortOptions = new List<BookingSortOptionViewModel>
                {
                    new BookingSortOptionViewModel { Value = "date", Text = "Найближча дата" },
                    new BookingSortOptionViewModel { Value = "newest", Text = "Нові заявки спочатку" }
                }
            };
        }

        private static IOrderedQueryable<Booking> ApplyBookingSort(IQueryable<Booking> query, string sort)
        {
            if (NormalizeSort(sort) == "newest")
            {
                return query
                    .OrderBy(booking => booking.Status == BookingStatus.Pending ? 0 : booking.Status == BookingStatus.InProgress ? 1 : booking.Status == BookingStatus.Confirmed ? 2 : booking.Status == BookingStatus.Completed ? 3 : 4)
                    .ThenByDescending(booking => booking.CreatedAt)
                    .ThenBy(booking => booking.ScheduledAt);
            }

            DateTime now = DateTime.UtcNow;

            return query
                .OrderBy(booking => booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Rejected)
                .ThenBy(booking => booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Rejected ? 0 : booking.ScheduledAt < now ? 1 : 0)
                .ThenBy(booking => booking.Status == BookingStatus.Completed ? 0 : booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Rejected ? 1 : 0)
                .ThenByDescending(booking => booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Rejected ? booking.ScheduledAt : DateTime.MinValue)
                .ThenBy(booking => booking.ScheduledAt)
                .ThenBy(booking => booking.Status == BookingStatus.InProgress ? 0 : booking.Status == BookingStatus.Confirmed ? 1 : booking.Status == BookingStatus.Pending ? 2 : booking.Status == BookingStatus.Completed ? 3 : 4)
                .ThenByDescending(booking => booking.CreatedAt);
        }

        private static string NormalizeSort(string? sort)
        {
            return sort == "newest" ? "newest" : "date";
        }

        private static int NormalizePage(int page, int totalItems)
        {
            if (page < 1)
            {
                return 1;
            }

            int totalPages = CalculateTotalPages(totalItems);

            if (totalPages == 0)
            {
                return 1;
            }

            return Math.Min(page, totalPages);
        }

        private static int CalculateTotalPages(int totalItems)
        {
            if (totalItems == 0)
            {
                return 0;
            }

            return (int)Math.Ceiling(totalItems / (double)BookingPageSize);
        }

        private static IQueryable<Booking> ApplyBookingFilters(
            IQueryable<Booking> query,
            string? search,
            BookingStatus? status,
            bool includeCustomerFields)
        {
            if (status.HasValue)
            {
                query = query.Where(booking => booking.Status == status.Value);
            }

            if (string.IsNullOrWhiteSpace(search))
            {
                return query;
            }

            string searchTerm = $"%{search.Trim()}%";
            bool isNumber = int.TryParse(search.Trim(), out int bookingId);

            query = query.Where(booking =>
                (isNumber && booking.Id == bookingId) ||
                EF.Functions.ILike(booking.AutoService.Name, searchTerm) ||
                (booking.ProblemDescription != null && EF.Functions.ILike(booking.ProblemDescription, searchTerm)) ||
                (booking.Vehicle != null && EF.Functions.ILike(booking.Vehicle.Make, searchTerm)) ||
                (booking.Vehicle != null && EF.Functions.ILike(booking.Vehicle.Model, searchTerm)) ||
                (booking.Vehicle != null && EF.Functions.ILike(booking.Vehicle.LicensePlate, searchTerm)) ||
                (booking.GuestVehicleMake != null && EF.Functions.ILike(booking.GuestVehicleMake, searchTerm)) ||
                (booking.GuestVehicleModel != null && EF.Functions.ILike(booking.GuestVehicleModel, searchTerm)) ||
                (booking.GuestVehicleLicensePlate != null && EF.Functions.ILike(booking.GuestVehicleLicensePlate, searchTerm)) ||
                (includeCustomerFields && EF.Functions.ILike(booking.CustomerName, searchTerm)) ||
                (includeCustomerFields && EF.Functions.ILike(booking.CustomerPhone, searchTerm)) ||
                (includeCustomerFields && booking.CustomerEmail != null && EF.Functions.ILike(booking.CustomerEmail, searchTerm)));

            return query;
        }

        private static BookingListItemViewModel CreateBookingListItem(Booking booking, bool isAdminView)
        {
            return new BookingListItemViewModel
            {
                Id = booking.Id,
                AutoServiceName = booking.AutoService.Name,
                AutoServicePrice = booking.AutoService.Price,
                ScheduledAt = booking.ScheduledAt.ToLocalTime(),
                CreatedAt = booking.CreatedAt.ToLocalTime(),
                StatusName = booking.Status.GetDisplayName(),
                StatusCssClass = GetStatusCssClass(booking.Status),
                IsClosedWithoutEstimate = booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Rejected,
                VehicleTitle = GetVehicleTitle(booking),
                CustomerName = booking.CustomerName,
                CustomerPhone = booking.CustomerPhone,
                CustomerEmail = booking.CustomerEmail,
                ProblemDescription = booking.ProblemDescription,
                FinalPrice = booking.FinalPrice,
                EstimatedPrice = booking.EstimatedPrice,
                EstimatedDurationMinutes = booking.EstimatedDurationMinutes,
                AdminComment = booking.AdminComment,
                CanCancel = !isAdminView && (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed),
                CanConfirm = isAdminView && booking.Status == BookingStatus.Pending,
                CanReject = isAdminView && (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed),
                CanReschedule = isAdminView && (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed),
                CanStartWork = isAdminView && booking.Status == BookingStatus.Confirmed,
                CanComplete = isAdminView && booking.Status == BookingStatus.InProgress
            };
        }

        private static string GetVehicleTitle(Booking booking)
        {
            if (booking.Vehicle != null)
            {
                return $"{booking.Vehicle.LicensePlate} — {booking.Vehicle.Make} {booking.Vehicle.Model} ({booking.Vehicle.Year})";
            }

            return $"{booking.GuestVehicleLicensePlate} — {booking.GuestVehicleMake} {booking.GuestVehicleModel} ({booking.GuestVehicleYear})";
        }

        private static string GetStatusCssClass(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.Pending => "status-pending",
                BookingStatus.Confirmed => "status-confirmed",
                BookingStatus.InProgress => "status-progress",
                BookingStatus.Completed => "status-completed",
                BookingStatus.Cancelled => "status-cancelled",
                BookingStatus.Rejected => "status-rejected",
                _ => "status-pending"
            };
        }
    }
}
