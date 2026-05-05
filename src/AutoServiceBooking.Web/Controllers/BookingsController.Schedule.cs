using AutoServiceBooking.Web.Extensions;
using AutoServiceBooking.Web.Models;
using AutoServiceBooking.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBooking.Web.Controllers
{
    public partial class BookingsController
    {
        private const int ScheduleDaysCount = 14;

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Schedule(DateTime? date)
        {
            DateTime today = DateTime.Today;
            DateTime selectedDate = (date ?? today).Date;
            DateTime fromDate = selectedDate < today ? selectedDate : today;
            DateTime toDate = fromDate.AddDays(ScheduleDaysCount - 1);

            (DateTime scheduleStartUtc, DateTime scheduleEndUtc) = _scheduleService.GetLocalDateUtcRange(fromDate, toDate.AddDays(1));

            List<Booking> scheduleBookings = await _dbContext.Bookings
                .Include(booking => booking.AutoService)
                .Include(booking => booking.Vehicle)
                .Where(booking => booking.ScheduledAt >= scheduleStartUtc && booking.ScheduledAt < scheduleEndUtc)
                .OrderBy(booking => booking.ScheduledAt)
                .ToListAsync();

            List<BlockedDate> blockedDates = await _dbContext.BlockedDates
                .Where(blockedDate => blockedDate.Date >= fromDate && blockedDate.Date <= toDate.AddDays(60))
                .OrderBy(blockedDate => blockedDate.Date)
                .ToListAsync();

            Dictionary<DateTime, BlockedDate> blockedDateMap = blockedDates
                .GroupBy(blockedDate => blockedDate.Date.Date)
                .ToDictionary(group => group.Key, group => group.First());

            Dictionary<DateTime, int> bookingCountByDate = scheduleBookings
                .GroupBy(booking => booking.ScheduledAt.ToLocalTime().Date)
                .ToDictionary(group => group.Key, group => group.Count());

            (DateTime selectedStartUtc, DateTime selectedEndUtc) = _scheduleService.GetLocalDateUtcRange(selectedDate, selectedDate.AddDays(1));

            SchedulePageViewModel model = new SchedulePageViewModel
            {
                SelectedDate = selectedDate,
                FromDate = fromDate,
                ToDate = toDate,
                NewBlockedDate = new BlockDateFormViewModel { Date = selectedDate < today ? today : selectedDate },
                Days = Enumerable.Range(0, ScheduleDaysCount)
                    .Select(index => fromDate.AddDays(index))
                    .Select(day => new ScheduleDayViewModel
                    {
                        Date = day,
                        BookingCount = bookingCountByDate.GetValueOrDefault(day),
                        IsSelected = day == selectedDate,
                        IsBlocked = blockedDateMap.ContainsKey(day),
                        BlockReason = blockedDateMap.TryGetValue(day, out BlockedDate? blockedDate) ? blockedDate.Reason : null
                    })
                    .ToList(),
                Bookings = scheduleBookings
                    .Where(booking => booking.ScheduledAt >= selectedStartUtc && booking.ScheduledAt < selectedEndUtc)
                    .Select(booking => new ScheduleBookingViewModel
                    {
                        Id = booking.Id,
                        ScheduledAt = booking.ScheduledAt.ToLocalTime(),
                        CustomerName = booking.CustomerName,
                        CustomerPhone = booking.CustomerPhone,
                        AutoServiceName = booking.AutoService.Name,
                        VehicleTitle = GetVehicleTitle(booking),
                        StatusName = booking.Status.GetDisplayName(),
                        StatusCssClass = GetStatusCssClass(booking.Status)
                    })
                    .ToList(),
                BlockedDates = blockedDates
                    .Select(blockedDate => new BlockedDateListItemViewModel
                    {
                        Id = blockedDate.Id,
                        Date = blockedDate.Date,
                        Reason = blockedDate.Reason
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockDate(BlockDateFormViewModel formModel)
        {
            DateTime date = formModel.Date.Date;

            if (date < DateTime.Today)
            {
                TempData["ErrorMessage"] = "Не можна заблокувати дату в минулому.";
                return RedirectToAction(nameof(Schedule));
            }

            if (await _dbContext.BlockedDates.AnyAsync(blockedDate => blockedDate.Date == date))
            {
                TempData["ErrorMessage"] = "Ця дата вже заблокована.";
                return RedirectToAction(nameof(Schedule), new { date = date.ToString("yyyy-MM-dd") });
            }

            try
            {
                _dbContext.BlockedDates.Add(new BlockedDate(date, formModel.Reason));
                await _dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Дату заблоковано для нових записів.";
            }
            catch (ArgumentException exception)
            {
                TempData["ErrorMessage"] = exception.Message;
            }

            return RedirectToAction(nameof(Schedule), new { date = date.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockDate(int id)
        {
            BlockedDate? blockedDate = await _dbContext.BlockedDates.FirstOrDefaultAsync(currentDate => currentDate.Id == id);

            if (blockedDate == null)
            {
                return NotFound();
            }

            DateTime date = blockedDate.Date;
            _dbContext.BlockedDates.Remove(blockedDate);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Дату знову відкрито для записів.";
            return RedirectToAction(nameof(Schedule), new { date = date.ToString("yyyy-MM-dd") });
        }

        private async Task FillAdminAvailabilityAsync(BookingListPageViewModel model)
        {
            model.WorkDayStartHour = _scheduleService.WorkDayStartHour;
            model.WorkDayEndHour = _scheduleService.WorkDayEndHour;
            model.SlotStepMinutes = _scheduleService.SlotStepMinutes;

            DateTime fromDate = DateTime.Today;
            DateTime toDate = fromDate.AddDays(30);

            List<BlockedDate> blockedDates = await _scheduleService.GetBlockedDatesAsync(fromDate, toDate);

            model.BlockedDates = blockedDates
                .Select(blockedDate => new BookingBlockedDateViewModel
                {
                    DateValue = blockedDate.Date.ToString("yyyy-MM-dd"),
                    DisplayDate = blockedDate.Date.ToString("dd.MM.yyyy"),
                    Reason = blockedDate.Reason
                })
                .ToList();

            List<Booking> occupiedBookings = await _scheduleService.GetOccupiedBookingsAsync(fromDate, toDate);

            model.OccupiedIntervals = occupiedBookings
                .Select(booking =>
                {
                    DateTime localStart = booking.ScheduledAt.ToLocalTime();
                    DateTime localEnd = localStart.AddMinutes(_scheduleService.GetBookingDurationMinutes(booking));

                    return new BookingOccupiedIntervalViewModel
                    {
                        BookingId = booking.Id,
                        DateValue = localStart.ToString("yyyy-MM-dd"),
                        StartTime = localStart.ToString("HH:mm"),
                        EndTime = localEnd.ToString("HH:mm"),
                        Label = $"Заявка #{booking.Id}"
                    };
                })
                .ToList();
        }

    }
}
