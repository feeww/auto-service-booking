using AutoServiceBooking.Web.Models;

namespace AutoServiceBooking.Web.Services.Scheduling
{
    public interface IScheduleService
    {
        int WorkDayStartHour { get; }

        int WorkDayEndHour { get; }

        int SlotStepMinutes { get; }

        int GetBookingDurationMinutes(Booking booking);

        string? ValidateWorkingHours(DateTime scheduledAt, int durationMinutes);

        Task<BlockedDate?> FindBlockedDateAsync(DateTime localDate);

        Task<Booking?> FindOverlappingBookingAsync(DateTime scheduledAtUtc, int durationMinutes, int? ignoredBookingId = null);

        Task<List<BlockedDate>> GetBlockedDatesAsync(DateTime fromDate, DateTime toDate);

        Task<List<Booking>> GetOccupiedBookingsAsync(DateTime fromDate, DateTime toDate);

        (DateTime StartUtc, DateTime EndUtc) GetLocalDateUtcRange(DateTime localStartDate, DateTime localEndDate);
    }
}
