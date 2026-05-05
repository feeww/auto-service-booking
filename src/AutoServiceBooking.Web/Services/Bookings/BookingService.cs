using AutoServiceBooking.Web.Data;
using AutoServiceBooking.Web.Models;
using AutoServiceBooking.Web.Services.Scheduling;
using AutoServiceBooking.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBooking.Web.Services.Bookings
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IScheduleService _scheduleService;

        public BookingService(ApplicationDbContext dbContext, IScheduleService scheduleService)
        {
            _dbContext = dbContext;
            _scheduleService = scheduleService;
        }

        public async Task<BookingOperationResult> CreateAsync(BookingCreateViewModel formModel, int? clientUserId)
        {
            DateTime scheduledAtUtc = DateTime.SpecifyKind(formModel.ScheduledAt, DateTimeKind.Local).ToUniversalTime();

            try
            {
                if (clientUserId.HasValue)
                {
                    return await CreateAuthenticatedBookingAsync(formModel, scheduledAtUtc, clientUserId.Value);
                }

                Booking booking = CreateGuestBooking(formModel, scheduledAtUtc);
                _dbContext.Bookings.Add(booking);
                await _dbContext.SaveChangesAsync();

                return BookingOperationResult.Ok();
            }
            catch (ArgumentException exception)
            {
                return BookingOperationResult.Fail(exception.Message);
            }
        }

        public async Task<BookingOperationResult> ConfirmAsync(int id, decimal estimatedPrice, int estimatedDurationMinutes)
        {
            Booking? booking = await _dbContext.Bookings
                .Include(currentBooking => currentBooking.AutoService)
                .FirstOrDefaultAsync(currentBooking => currentBooking.Id == id);

            if (booking == null)
            {
                return BookingOperationResult.Missing();
            }

            string? availabilityError = await ValidateScheduleChangeAsync(booking, booking.ScheduledAt.ToLocalTime(), estimatedDurationMinutes);
            if (availabilityError != null)
            {
                return BookingOperationResult.Fail(availabilityError);
            }

            return await ChangeBookingAsync(booking, currentBooking => currentBooking.Confirm(estimatedPrice, estimatedDurationMinutes));
        }

        public async Task<BookingOperationResult> RejectAsync(int id)
        {
            return await ChangeBookingAsync(id, booking => booking.Reject());
        }

        public async Task<BookingOperationResult> StartWorkAsync(int id)
        {
            return await ChangeBookingAsync(id, booking => booking.StartWork());
        }

        public async Task<BookingOperationResult> CompleteAsync(int id, decimal finalPrice, string? adminComment)
        {
            return await ChangeBookingAsync(id, booking => booking.Complete(finalPrice, adminComment));
        }

        public async Task<BookingOperationResult> RescheduleAsync(int id, DateTime scheduledAt)
        {
            if (scheduledAt <= DateTime.Now)
            {
                return BookingOperationResult.Fail("Оберіть майбутню дату та час.");
            }

            Booking? booking = await _dbContext.Bookings
                .Include(currentBooking => currentBooking.AutoService)
                .FirstOrDefaultAsync(currentBooking => currentBooking.Id == id);

            if (booking == null)
            {
                return BookingOperationResult.Missing();
            }

            int durationMinutes = _scheduleService.GetBookingDurationMinutes(booking);
            string? availabilityError = await ValidateScheduleChangeAsync(booking, scheduledAt, durationMinutes);
            if (availabilityError != null)
            {
                return BookingOperationResult.Fail(availabilityError);
            }

            DateTime scheduledAtUtc = DateTime.SpecifyKind(scheduledAt, DateTimeKind.Local).ToUniversalTime();
            return await ChangeBookingAsync(booking, currentBooking => currentBooking.Reschedule(scheduledAtUtc));
        }

        public async Task<BookingOperationResult> CancelAsync(int id, int clientUserId)
        {
            Booking? booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(currentBooking => currentBooking.Id == id && currentBooking.ClientUserId == clientUserId);

            if (booking == null)
            {
                return BookingOperationResult.Missing();
            }

            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
            {
                return BookingOperationResult.Fail("Скасувати можна тільки записи, які очікують або підтверджені.");
            }

            return await ChangeBookingAsync(booking, currentBooking => currentBooking.Cancel());
        }

        private async Task<BookingOperationResult> CreateAuthenticatedBookingAsync(BookingCreateViewModel formModel, DateTime scheduledAtUtc, int clientUserId)
        {
            if (formModel.SelectedVehicleId.HasValue)
            {
                Vehicle? savedVehicle = await _dbContext.Vehicles
                    .FirstOrDefaultAsync(vehicle =>
                        vehicle.Id == formModel.SelectedVehicleId.Value &&
                        vehicle.ClientUserId == clientUserId &&
                        !vehicle.IsArchived);

                if (savedVehicle == null)
                {
                    return BookingOperationResult.Fail("Оберіть активний автомобіль зі списку.", nameof(formModel.SelectedVehicleId));
                }

                Booking booking = new Booking(
                    clientUserId,
                    savedVehicle.Id,
                    formModel.AutoServiceId!.Value,
                    scheduledAtUtc,
                    formModel.ProblemDescription,
                    formModel.CustomerName,
                    formModel.CustomerPhone,
                    formModel.CustomerEmail);

                _dbContext.Bookings.Add(booking);
                await _dbContext.SaveChangesAsync();
                return BookingOperationResult.Ok();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            Vehicle vehicle = new Vehicle(
                clientUserId,
                formModel.VehicleMake!,
                formModel.VehicleModel!,
                formModel.VehicleYear!.Value,
                formModel.VehicleLicensePlate!,
                formModel.VehicleMileage!.Value,
                formModel.VehicleFuelType);

            _dbContext.Vehicles.Add(vehicle);
            await _dbContext.SaveChangesAsync();

            Booking newBooking = new Booking(
                clientUserId,
                vehicle.Id,
                formModel.AutoServiceId!.Value,
                scheduledAtUtc,
                formModel.ProblemDescription,
                formModel.CustomerName,
                formModel.CustomerPhone,
                formModel.CustomerEmail);

            _dbContext.Bookings.Add(newBooking);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return BookingOperationResult.Ok();
        }

        private static Booking CreateGuestBooking(BookingCreateViewModel formModel, DateTime scheduledAtUtc)
        {
            return new Booking(
                formModel.AutoServiceId!.Value,
                scheduledAtUtc,
                formModel.ProblemDescription,
                formModel.CustomerName,
                formModel.CustomerPhone,
                formModel.CustomerEmail,
                formModel.VehicleMake!,
                formModel.VehicleModel!,
                formModel.VehicleYear!.Value,
                formModel.VehicleLicensePlate!,
                formModel.VehicleMileage!.Value,
                formModel.VehicleFuelType);
        }

        private async Task<BookingOperationResult> ChangeBookingAsync(int id, Action<Booking> changeBooking)
        {
            Booking? booking = await _dbContext.Bookings.FirstOrDefaultAsync(currentBooking => currentBooking.Id == id);

            if (booking == null)
            {
                return BookingOperationResult.Missing();
            }

            return await ChangeBookingAsync(booking, changeBooking);
        }

        private async Task<BookingOperationResult> ChangeBookingAsync(Booking booking, Action<Booking> changeBooking)
        {
            try
            {
                changeBooking(booking);
                await _dbContext.SaveChangesAsync();
                return BookingOperationResult.Ok();
            }
            catch (Exception exception) when (exception is InvalidOperationException || exception is ArgumentException)
            {
                return BookingOperationResult.Fail(exception.Message);
            }
        }

        private async Task<string?> ValidateScheduleChangeAsync(Booking booking, DateTime scheduledAt, int durationMinutes)
        {
            string? workingHoursError = _scheduleService.ValidateWorkingHours(scheduledAt, durationMinutes);
            if (workingHoursError != null)
            {
                return workingHoursError;
            }

            BlockedDate? blockedDate = await _scheduleService.FindBlockedDateAsync(scheduledAt);
            if (blockedDate != null)
            {
                return $"На {blockedDate.Date:dd.MM.yyyy} запис недоступний. Причина: {blockedDate.Reason}.";
            }

            DateTime scheduledAtUtc = DateTime.SpecifyKind(scheduledAt, DateTimeKind.Local).ToUniversalTime();
            Booking? overlappingBooking = await _scheduleService.FindOverlappingBookingAsync(scheduledAtUtc, durationMinutes, booking.Id);

            if (overlappingBooking != null)
            {
                DateTime busyStart = overlappingBooking.ScheduledAt.ToLocalTime();
                DateTime busyEnd = busyStart.AddMinutes(_scheduleService.GetBookingDurationMinutes(overlappingBooking));
                return $"Обраний час перетинається із заявкою #{overlappingBooking.Id}: {busyStart:HH:mm}–{busyEnd:HH:mm}.";
            }

            return null;
        }
    }
}
