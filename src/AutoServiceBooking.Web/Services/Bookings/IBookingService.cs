using AutoServiceBooking.Web.ViewModels;

namespace AutoServiceBooking.Web.Services.Bookings
{
    public interface IBookingService
    {
        Task<BookingOperationResult> CreateAsync(BookingCreateViewModel formModel, int? clientUserId);

        Task<BookingOperationResult> ConfirmAsync(int id, decimal estimatedPrice, int estimatedDurationMinutes);

        Task<BookingOperationResult> RejectAsync(int id);

        Task<BookingOperationResult> StartWorkAsync(int id);

        Task<BookingOperationResult> CompleteAsync(int id, decimal finalPrice, string? adminComment);

        Task<BookingOperationResult> RescheduleAsync(int id, DateTime scheduledAt);

        Task<BookingOperationResult> CancelAsync(int id, int clientUserId);
    }
}
