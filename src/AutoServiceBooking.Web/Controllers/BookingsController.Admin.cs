using AutoServiceBooking.Web.Models;
using AutoServiceBooking.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBooking.Web.Controllers
{
    public partial class BookingsController
    {
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin(string? search, BookingStatus? status, string sort = "nearest", int page = 1)
        {
            IQueryable<Booking> query = _dbContext.Bookings
                .Include(booking => booking.AutoService)
                .Include(booking => booking.Vehicle);

            query = ApplyBookingFilters(query, search, status, true);

            int totalBookings = await query.CountAsync();
            int pageNumber = NormalizePage(page, totalBookings);

            List<Booking> bookings = await ApplyBookingSort(query, sort)
                .Skip((pageNumber - 1) * BookingPageSize)
                .Take(BookingPageSize)
                .ToListAsync();

            BookingListPageViewModel model = CreateBookingListPage(bookings, search, status, sort, true, pageNumber, totalBookings);
            await FillAdminAvailabilityAsync(model);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id, decimal estimatedPrice, int estimatedDurationMinutes)
        {
            Booking? booking = await _dbContext.Bookings
                .Include(currentBooking => currentBooking.AutoService)
                .FirstOrDefaultAsync(currentBooking => currentBooking.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            string? availabilityError = await ValidateAdminScheduleChangeAsync(booking, booking.ScheduledAt.ToLocalTime(), estimatedDurationMinutes);
            if (availabilityError != null)
            {
                TempData["ErrorMessage"] = availabilityError;
                return RedirectToAction(nameof(Admin));
            }

            try
            {
                booking.Confirm(estimatedPrice, estimatedDurationMinutes);
                await _dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Запис підтверджено.";
            }
            catch (Exception exception) when (exception is InvalidOperationException || exception is ArgumentException)
            {
                TempData["ErrorMessage"] = exception.Message;
            }

            return RedirectToAction(nameof(Admin));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            return await ChangeAdminBookingAsync(id, booking => booking.Reject(), "Запис відхилено.");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartWork(int id)
        {
            return await ChangeAdminBookingAsync(id, booking => booking.StartWork(), "Запис переведено в роботу.");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, decimal finalPrice, string? adminComment)
        {
            return await ChangeAdminBookingAsync(id, booking => booking.Complete(finalPrice, adminComment), "Запис завершено.");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(int id, DateTime scheduledAt)
        {
            if (scheduledAt <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "Оберіть майбутню дату та час.";
                return RedirectToAction(nameof(Admin));
            }

            Booking? booking = await _dbContext.Bookings
                .Include(currentBooking => currentBooking.AutoService)
                .FirstOrDefaultAsync(currentBooking => currentBooking.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            int durationMinutes = GetBookingDurationMinutes(booking);
            string? availabilityError = await ValidateAdminScheduleChangeAsync(booking, scheduledAt, durationMinutes);
            if (availabilityError != null)
            {
                TempData["ErrorMessage"] = availabilityError;
                return RedirectToAction(nameof(Admin));
            }

            DateTime scheduledAtUtc = DateTime.SpecifyKind(scheduledAt, DateTimeKind.Local).ToUniversalTime();

            try
            {
                booking.Reschedule(scheduledAtUtc);
                await _dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Запис перенесено.";
            }
            catch (Exception exception) when (exception is InvalidOperationException || exception is ArgumentException)
            {
                TempData["ErrorMessage"] = exception.Message;
            }

            return RedirectToAction(nameof(Admin));
        }

        private async Task<string?> ValidateAdminScheduleChangeAsync(Booking booking, DateTime scheduledAt, int durationMinutes)
        {
            string? workingHoursError = ValidateWorkingHours(scheduledAt, durationMinutes);
            if (workingHoursError != null)
            {
                return workingHoursError;
            }

            BlockedDate? blockedDate = await FindBlockedDateAsync(scheduledAt);
            if (blockedDate != null)
            {
                return $"На {blockedDate.Date:dd.MM.yyyy} запис недоступний. Причина: {blockedDate.Reason}.";
            }

            DateTime scheduledAtUtc = DateTime.SpecifyKind(scheduledAt, DateTimeKind.Local).ToUniversalTime();
            Booking? overlappingBooking = await FindOverlappingBookingAsync(scheduledAtUtc, durationMinutes, booking.Id);

            if (overlappingBooking != null)
            {
                DateTime busyStart = overlappingBooking.ScheduledAt.ToLocalTime();
                DateTime busyEnd = busyStart.AddMinutes(GetBookingDurationMinutes(overlappingBooking));
                return $"Обраний час перетинається із заявкою #{overlappingBooking.Id}: {busyStart:HH:mm}–{busyEnd:HH:mm}.";
            }

            return null;
        }

        private async Task<IActionResult> ChangeAdminBookingAsync(int id, Action<Booking> changeStatus, string successMessage)
        {
            Booking? booking = await _dbContext.Bookings.FirstOrDefaultAsync(currentBooking => currentBooking.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            try
            {
                changeStatus(booking);
                await _dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = successMessage;
            }
            catch (Exception exception) when (exception is InvalidOperationException || exception is ArgumentException)
            {
                TempData["ErrorMessage"] = exception.Message;
            }

            return RedirectToAction(nameof(Admin));
        }
    }
}
