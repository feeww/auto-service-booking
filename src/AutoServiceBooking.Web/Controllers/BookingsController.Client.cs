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
        public async Task<IActionResult> Index(string? search, BookingStatus? status, string sort = "nearest", int page = 1)
        {
            if (User.IsInRole(UserRole.Admin.ToString()))
            {
                return RedirectToAction(nameof(Admin));
            }

            int clientUserId = User.GetUserId();
            IQueryable<Booking> query = _dbContext.Bookings
                .Include(booking => booking.AutoService)
                .Include(booking => booking.Vehicle)
                .Where(booking => booking.ClientUserId == clientUserId);

            query = ApplyBookingFilters(query, search, status, false);

            int totalBookings = await query.CountAsync();
            int pageNumber = NormalizePage(page, totalBookings);

            List<Booking> bookings = await ApplyBookingSort(query, sort)
                .Skip((pageNumber - 1) * BookingPageSize)
                .Take(BookingPageSize)
                .ToListAsync();

            BookingListPageViewModel model = CreateBookingListPage(bookings, search, status, sort, false, pageNumber, totalBookings);

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
