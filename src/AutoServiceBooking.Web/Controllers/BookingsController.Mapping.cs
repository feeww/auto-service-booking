using AutoServiceBooking.Web.Extensions;
using AutoServiceBooking.Web.Models;
using AutoServiceBooking.Web.ViewModels;

namespace AutoServiceBooking.Web.Controllers
{
    public partial class BookingsController
    {
        private static BookingListItemViewModel CreateBookingListItem(Booking booking, bool isAdminView)
        {
            return new BookingListItemViewModel
            {
                Id = booking.Id,
                AutoServiceName = booking.AutoService.Name,
                ScheduledAt = booking.ScheduledAt.ToLocalTime(),
                CreatedAt = booking.CreatedAt.ToLocalTime(),
                StatusName = booking.Status.GetDisplayName(),
                StatusCssClass = GetStatusCssClass(booking.Status),
                VehicleTitle = GetVehicleTitle(booking),
                CustomerName = booking.CustomerName,
                CustomerPhone = booking.CustomerPhone,
                CustomerEmail = booking.CustomerEmail,
                ProblemDescription = booking.ProblemDescription,
                FinalPrice = booking.FinalPrice,
                AdminComment = booking.AdminComment,
                CanCancel = !isAdminView && (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed),
                CanConfirm = isAdminView && booking.Status == BookingStatus.Pending,
                CanReject = isAdminView && booking.Status == BookingStatus.Pending,
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
