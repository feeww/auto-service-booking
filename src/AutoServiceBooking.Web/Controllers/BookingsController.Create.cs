using AutoServiceBooking.Web.Extensions;
using AutoServiceBooking.Web.Models;
using AutoServiceBooking.Web.Models.Users;
using AutoServiceBooking.Web.Services.Bookings;
using AutoServiceBooking.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBooking.Web.Controllers
{
    public partial class BookingsController
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Create()
        {
            BookingCreateViewModel model = new BookingCreateViewModel();
            await PrefillCustomerAsync(model);
            await FillOptionsAsync(model);

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingCreateViewModel formModel)
        {
            await FillOptionsAsync(formModel);
            await ValidateBookingFormAsync(formModel);

            if (!ModelState.IsValid)
            {
                return View(formModel);
            }

            int? clientUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
            BookingOperationResult result = await _bookingService.CreateAsync(formModel, clientUserId);

            if (!result.Success)
            {
                ModelState.AddModelError(result.FieldName ?? string.Empty, result.ErrorMessage ?? "Не вдалося створити запис.");
                return View(formModel);
            }

            TempData["SuccessMessage"] = "Запис створено. Менеджер зв'яжеться з вами для підтвердження.";
            return RedirectToAction(nameof(Success));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Success()
        {
            return View();
        }

        private async Task PrefillCustomerAsync(BookingCreateViewModel model)
        {
            model.IsAuthenticatedUser = User.Identity?.IsAuthenticated == true;

            if (!model.IsAuthenticatedUser)
            {
                return;
            }

            int clientUserId = User.GetUserId();
            AppUser? user = await _dbContext.Users.FirstOrDefaultAsync(appUser => appUser.Id == clientUserId);

            if (user == null)
            {
                return;
            }

            model.CustomerName = user.FullName;
            model.CustomerPhone = user.PhoneNumber ?? string.Empty;
            model.CustomerEmail = user.Email;
        }

        private async Task FillOptionsAsync(BookingCreateViewModel model)
        {
            model.IsAuthenticatedUser = User.Identity?.IsAuthenticated == true;
            model.MinScheduledAt = DateTime.Now.AddMinutes(30);
            model.WorkDayStartHour = _scheduleService.WorkDayStartHour;
            model.WorkDayEndHour = _scheduleService.WorkDayEndHour;
            model.SlotStepMinutes = _scheduleService.SlotStepMinutes;

            List<BlockedDate> blockedDates = await _dbContext.BlockedDates
                .Where(blockedDate => blockedDate.Date >= DateTime.Today)
                .OrderBy(blockedDate => blockedDate.Date)
                .Take(30)
                .ToListAsync();

            model.BlockedDates = blockedDates
                .Select(blockedDate => new BookingBlockedDateViewModel
                {
                    DateValue = blockedDate.Date.ToString("yyyy-MM-dd"),
                    DisplayDate = blockedDate.Date.ToString("dd.MM.yyyy"),
                    Reason = blockedDate.Reason
                })
                .ToList();

            List<AutoService> activeServices = await _dbContext.AutoServices
                .Where(autoService => autoService.IsActive)
                .OrderBy(autoService => autoService.Name == "Інше")
                .ThenBy(autoService => autoService.Name)
                .ToListAsync();

            model.ServiceDurations = activeServices
                .Select(autoService => new BookingServiceDurationViewModel
                {
                    ServiceId = autoService.Id,
                    DurationMinutes = autoService.DurationMinutes
                })
                .ToList();

            model.AutoServiceOptions = activeServices
                .Select(autoService => new SelectListItem
                {
                    Value = autoService.Id.ToString(),
                    Text = autoService.Price == 0
                        ? $"{autoService.Name} — опишіть проблему"
                        : $"{autoService.Name} — від {autoService.Price:0} грн"
                })
                .ToList();

            DateTime availabilityFrom = DateTime.Today;
            DateTime availabilityTo = availabilityFrom.AddDays(30);
            List<Booking> occupiedBookings = await _scheduleService.GetOccupiedBookingsAsync(availabilityFrom, availabilityTo);

            model.UnavailableSlots = CreatePublicUnavailableSlots(activeServices, occupiedBookings, availabilityFrom, availabilityTo);

            if (!model.IsAuthenticatedUser)
            {
                return;
            }

            int clientUserId = User.GetUserId();
            List<Vehicle> savedVehicles = await _dbContext.Vehicles
                .Where(vehicle => vehicle.ClientUserId == clientUserId && !vehicle.IsArchived)
                .OrderByDescending(vehicle => vehicle.CreatedAt)
                .ToListAsync();

            model.SavedVehicleOptions = savedVehicles
                .Select(vehicle => new SelectListItem
                {
                    Value = vehicle.Id.ToString(),
                    Text = $"{vehicle.LicensePlate} — {vehicle.Make} {vehicle.Model} ({vehicle.Year})"
                })
                .ToList();

            model.SavedVehicles = savedVehicles
                .Select(vehicle => new SavedVehicleBookingViewModel
                {
                    Id = vehicle.Id,
                    Make = vehicle.Make,
                    Model = vehicle.Model,
                    Year = vehicle.Year,
                    LicensePlate = vehicle.LicensePlate,
                    Mileage = vehicle.Mileage,
                    FuelTypeName = vehicle.FuelType.GetDisplayName()
                })
                .ToList();
        }

        private List<BookingUnavailableSlotViewModel> CreatePublicUnavailableSlots(
            List<AutoService> activeServices,
            List<Booking> occupiedBookings,
            DateTime fromDate,
            DateTime toDate)
        {
            List<BookingUnavailableSlotViewModel> unavailableSlots = new List<BookingUnavailableSlotViewModel>();
            List<(DateTime Date, int StartMinutes, int EndMinutes)> occupiedIntervals = occupiedBookings
                .Select(booking =>
                {
                    DateTime localStart = booking.ScheduledAt.ToLocalTime();
                    DateTime localEnd = localStart.AddMinutes(_scheduleService.GetBookingDurationMinutes(booking));
                    return (localStart.Date, localStart.Hour * 60 + localStart.Minute, localEnd.Hour * 60 + localEnd.Minute);
                })
                .ToList();

            for (DateTime date = fromDate.Date; date < toDate.Date; date = date.AddDays(1))
            {
                foreach (AutoService activeService in activeServices)
                {
                    for (int start = _scheduleService.WorkDayStartHour * 60; start + activeService.DurationMinutes <= _scheduleService.WorkDayEndHour * 60; start += _scheduleService.SlotStepMinutes)
                    {
                        int end = start + activeService.DurationMinutes;
                        bool isUnavailable = occupiedIntervals.Any(interval =>
                            interval.Date == date &&
                            start < interval.EndMinutes &&
                            end > interval.StartMinutes);

                        if (!isUnavailable)
                        {
                            continue;
                        }

                        unavailableSlots.Add(new BookingUnavailableSlotViewModel
                        {
                            ServiceId = activeService.Id,
                            DateValue = date.ToString("yyyy-MM-dd"),
                            TimeValue = TimeSpan.FromMinutes(start).ToString(@"hh\:mm")
                        });
                    }
                }
            }

            return unavailableSlots;
        }

        private async Task ValidateBookingFormAsync(BookingCreateViewModel formModel)
        {
            AutoService? selectedService = null;

            if (!formModel.AutoServiceId.HasValue)
            {
                if (!ModelState.TryGetValue(nameof(formModel.AutoServiceId), out var serviceState) || serviceState.Errors.Count == 0)
                {
                    ModelState.AddModelError(nameof(formModel.AutoServiceId), "Оберіть послугу.");
                }
            }
            else
            {
                selectedService = await _dbContext.AutoServices
                    .FirstOrDefaultAsync(autoService => autoService.Id == formModel.AutoServiceId.Value && autoService.IsActive);

                if (selectedService == null)
                {
                    ModelState.AddModelError(nameof(formModel.AutoServiceId), "Оберіть активну послугу.");
                }
            }

            if (formModel.ScheduledAt <= DateTime.Now)
            {
                ModelState.AddModelError(nameof(formModel.ScheduledAt), "Оберіть майбутню дату та час.");
            }

            if (selectedService != null)
            {
                string? workingHoursError = _scheduleService.ValidateWorkingHours(formModel.ScheduledAt, selectedService.DurationMinutes);
                if (workingHoursError != null)
                {
                    ModelState.AddModelError(nameof(formModel.ScheduledAt), workingHoursError);
                }
            }

            BlockedDate? blockedDate = await _scheduleService.FindBlockedDateAsync(formModel.ScheduledAt);
            if (blockedDate != null)
            {
                ModelState.AddModelError(nameof(formModel.ScheduledAt), $"На {blockedDate.Date:dd.MM.yyyy} запис недоступний. Причина: {blockedDate.Reason}.");
            }

            if (selectedService != null)
            {
                DateTime scheduledAtUtc = DateTime.SpecifyKind(formModel.ScheduledAt, DateTimeKind.Local).ToUniversalTime();
                Booking? overlappingBooking = await _scheduleService.FindOverlappingBookingAsync(scheduledAtUtc, selectedService.DurationMinutes);

                if (overlappingBooking != null)
                {
                    ModelState.AddModelError(nameof(formModel.ScheduledAt), "Цей час уже зайнятий. Оберіть інший доступний час.");
                }
            }

            bool canUseSavedVehicle = User.Identity?.IsAuthenticated == true && formModel.SelectedVehicleId.HasValue;

            if (canUseSavedVehicle)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(formModel.VehicleMake))
            {
                ModelState.AddModelError(nameof(formModel.VehicleMake), "Вкажіть марку автомобіля.");
            }

            if (string.IsNullOrWhiteSpace(formModel.VehicleModel))
            {
                ModelState.AddModelError(nameof(formModel.VehicleModel), "Вкажіть модель автомобіля.");
            }

            if (!formModel.VehicleYear.HasValue || formModel.VehicleYear < 1900 || formModel.VehicleYear > DateTime.UtcNow.Year + 1)
            {
                ModelState.AddModelError(nameof(formModel.VehicleYear), "Вкажіть коректний рік випуску.");
            }

            if (string.IsNullOrWhiteSpace(formModel.VehicleLicensePlate))
            {
                ModelState.AddModelError(nameof(formModel.VehicleLicensePlate), "Вкажіть номерний знак.");
            }

            if (!formModel.VehicleMileage.HasValue || formModel.VehicleMileage < 0)
            {
                ModelState.AddModelError(nameof(formModel.VehicleMileage), "Вкажіть коректний пробіг.");
            }
        }
    }
}
