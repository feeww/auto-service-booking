using AutoServiceBooking.Web.Models;
using AutoServiceBooking.Web.Services.Bookings;
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
            BookingOperationResult result = await _bookingService.ConfirmAsync(id, estimatedPrice, estimatedDurationMinutes);
            return HandleAdminBookingResult(result, "Запис підтверджено.");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            BookingOperationResult result = await _bookingService.RejectAsync(id);
            return HandleAdminBookingResult(result, "Запис відхилено.");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartWork(int id)
        {
            BookingOperationResult result = await _bookingService.StartWorkAsync(id);
            return HandleAdminBookingResult(result, "Запис переведено в роботу.");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, decimal finalPrice, string? adminComment)
        {
            BookingOperationResult result = await _bookingService.CompleteAsync(id, finalPrice, adminComment);
            return HandleAdminBookingResult(result, "Запис завершено.");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(int id, DateTime scheduledAt)
        {
            BookingOperationResult result = await _bookingService.RescheduleAsync(id, scheduledAt);
            return HandleAdminBookingResult(result, "Запис перенесено.");
        }

        private IActionResult HandleAdminBookingResult(BookingOperationResult result, string successMessage)
        {
            if (result.NotFound)
            {
                return NotFound();
            }

            if (result.Success)
            {
                TempData["SuccessMessage"] = successMessage;
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Не вдалося виконати дію із записом.";
            }

            return RedirectToAction(nameof(Admin));
        }
    }
}
