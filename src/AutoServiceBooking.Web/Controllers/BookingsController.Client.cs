using AutoServiceBooking.Web.Extensions;
using AutoServiceBooking.Web.Models;
using AutoServiceBooking.Web.Models.Users;
using AutoServiceBooking.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBooking.Web.Controllers
{
    public partial class BookingsController
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(UserRole.Admin.ToString()))
            {
                return RedirectToAction(nameof(Admin));
            }

            int clientUserId = User.GetUserId();
            List<Booking> bookings = await _dbContext.Bookings
                .Include(booking => booking.AutoService)
                .Include(booking => booking.Vehicle)
                .Where(booking => booking.ClientUserId == clientUserId)
                .OrderByDescending(booking => booking.CreatedAt)
                .ToListAsync();

            List<BookingListItemViewModel> model = bookings
                .Select(booking => CreateBookingListItem(booking, false))
                .ToList();

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            int clientUserId = User.GetUserId();
            Booking? booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(currentBooking => currentBooking.Id == id && currentBooking.ClientUserId == clientUserId);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
            {
                TempData["ErrorMessage"] = "Скасувати можна тільки записи, які очікують або підтверджені.";
                return RedirectToAction(nameof(Index));
            }

            booking.Cancel();
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Запис скасовано.";
            return RedirectToAction(nameof(Index));
        }
    }
}
