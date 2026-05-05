using AutoServiceBooking.Web.Data;
using AutoServiceBooking.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBooking.Web.Services.Scheduling
{
    public class ScheduleService : IScheduleService
    {
        private readonly ApplicationDbContext _dbContext;

        public ScheduleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int WorkDayStartHour => 9;

        public int WorkDayEndHour => 19;

        public int SlotStepMinutes => 30;

        public int GetBookingDurationMinutes(Booking booking)
        {
            return booking.EstimatedDurationMinutes ?? booking.AutoService.DurationMinutes;
        }

        public string? ValidateWorkingHours(DateTime scheduledAt, int durationMinutes)
        {
            if (durationMinutes <= 0)
            {
                return "Тривалість запису має бути більшою за 0 хвилин.";
            }

            DateTime localScheduledAt = scheduledAt.Kind == DateTimeKind.Utc ? scheduledAt.ToLocalTime() : scheduledAt;

            if (localScheduledAt.DayOfWeek == DayOfWeek.Sunday)
            {
                return "У неділю сервіс не працює. Оберіть інший день.";
            }

            DateTime workStart = localScheduledAt.Date.AddHours(WorkDayStartHour);
            DateTime workEnd = localScheduledAt.Date.AddHours(WorkDayEndHour);
            DateTime bookingEnd = localScheduledAt.AddMinutes(durationMinutes);

            if (localScheduledAt.Minute % SlotStepMinutes != 0 || localScheduledAt.Second != 0)
            {
                return $"Оберіть час з кроком {SlotStepMinutes} хвилин.";
            }

            if (localScheduledAt < workStart || bookingEnd > workEnd)
            {
                return $"Запис доступний тільки в робочий час: {WorkDayStartHour:00}:00–{WorkDayEndHour:00}:00.";
            }

            return null;
        }

        public async Task<BlockedDate?> FindBlockedDateAsync(DateTime localDate)
        {
            DateTime date = localDate.Date;
            return await _dbContext.BlockedDates.FirstOrDefaultAsync(blockedDate => blockedDate.Date == date);
        }

        public async Task<Booking?> FindOverlappingBookingAsync(DateTime scheduledAtUtc, int durationMinutes, int? ignoredBookingId = null)
        {
            DateTime localDate = scheduledAtUtc.ToLocalTime().Date;
            (DateTime dayStartUtc, DateTime dayEndUtc) = GetLocalDateUtcRange(localDate, localDate.AddDays(1));
            DateTime requestedEndUtc = scheduledAtUtc.AddMinutes(durationMinutes);

            List<Booking> bookings = await _dbContext.Bookings
                .Include(booking => booking.AutoService)
                .Where(booking =>
                    (booking.Status == BookingStatus.Confirmed || booking.Status == BookingStatus.InProgress) &&
                    booking.ScheduledAt >= dayStartUtc &&
                    booking.ScheduledAt < dayEndUtc &&
                    (!ignoredBookingId.HasValue || booking.Id != ignoredBookingId.Value))
                .ToListAsync();

            return bookings.FirstOrDefault(booking =>
            {
                DateTime existingEndUtc = booking.ScheduledAt.AddMinutes(GetBookingDurationMinutes(booking));
                return scheduledAtUtc < existingEndUtc && requestedEndUtc > booking.ScheduledAt;
            });
        }

        public async Task<List<BlockedDate>> GetBlockedDatesAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbContext.BlockedDates
                .Where(blockedDate => blockedDate.Date >= fromDate.Date && blockedDate.Date < toDate.Date)
                .OrderBy(blockedDate => blockedDate.Date)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetOccupiedBookingsAsync(DateTime fromDate, DateTime toDate)
        {
            (DateTime startUtc, DateTime endUtc) = GetLocalDateUtcRange(fromDate, toDate);

            return await _dbContext.Bookings
                .Include(booking => booking.AutoService)
                .Where(booking =>
                    (booking.Status == BookingStatus.Confirmed || booking.Status == BookingStatus.InProgress) &&
                    booking.ScheduledAt >= startUtc &&
                    booking.ScheduledAt < endUtc)
                .OrderBy(booking => booking.ScheduledAt)
                .ToListAsync();
        }

        public (DateTime StartUtc, DateTime EndUtc) GetLocalDateUtcRange(DateTime localStartDate, DateTime localEndDate)
        {
            DateTime localStart = DateTime.SpecifyKind(localStartDate.Date, DateTimeKind.Local);
            DateTime localEnd = DateTime.SpecifyKind(localEndDate.Date, DateTimeKind.Local);

            return (localStart.ToUniversalTime(), localEnd.ToUniversalTime());
        }
    }
}
