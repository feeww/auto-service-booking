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
        public async Task<IActionResult> Admin()
        {
            List<Booking> bookings = await _dbContext.Bookings
                .Include(booking => booking.AutoService)
                .Include(booking => booking.Vehicle)
                .OrderByDescending(booking => booking.CreatedAt)
                .ToListAsync();

            List<BookingListItemViewModel> model = bookings
                .Select(booking => CreateBookingListItem(booking, true))
                .ToList();

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            return await ChangeAdminBookingAsync(id, booking => booking.Confirm(), "Запис підтверджено.");
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
