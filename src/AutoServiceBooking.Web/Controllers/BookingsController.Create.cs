using AutoServiceBooking.Web.Extensions;
using AutoServiceBooking.Web.Models;
using AutoServiceBooking.Web.Models.Users;
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

            DateTime scheduledAtUtc = DateTime.SpecifyKind(formModel.ScheduledAt, DateTimeKind.Local).ToUniversalTime();

            try
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    bool bookingCreated = await CreateAuthenticatedBookingAsync(formModel, scheduledAtUtc);

                    if (!bookingCreated)
                    {
                        return View(formModel);
                    }
                }
                else
                {
                    Booking booking = CreateGuestBooking(formModel, scheduledAtUtc);
                    _dbContext.Bookings.Add(booking);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
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

        private async Task<bool> CreateAuthenticatedBookingAsync(BookingCreateViewModel formModel, DateTime scheduledAtUtc)
        {
            int clientUserId = User.GetUserId();

            if (formModel.SelectedVehicleId.HasValue)
            {
                Vehicle? savedVehicle = await _dbContext.Vehicles
                    .FirstOrDefaultAsync(vehicle => vehicle.Id == formModel.SelectedVehicleId.Value && vehicle.ClientUserId == clientUserId);

                if (savedVehicle == null)
                {
                    ModelState.AddModelError(nameof(formModel.SelectedVehicleId), "Оберіть власний автомобіль зі списку.");
                    return false;
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
                return true;
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

            return true;
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

            List<AutoService> activeServices = await _dbContext.AutoServices
                .Where(autoService => autoService.IsActive)
                .OrderBy(autoService => autoService.Name == "Інше")
                .ThenBy(autoService => autoService.Name)
                .ToListAsync();

            model.AutoServiceOptions = activeServices
                .Select(autoService => new SelectListItem
                {
                    Value = autoService.Id.ToString(),
                    Text = autoService.Price == 0
                        ? $"{autoService.Name} — опишіть проблему"
                        : $"{autoService.Name} — від {autoService.Price:0} грн"
                })
                .ToList();

            if (!model.IsAuthenticatedUser)
            {
                return;
            }

            int clientUserId = User.GetUserId();
            List<Vehicle> savedVehicles = await _dbContext.Vehicles
                .Where(vehicle => vehicle.ClientUserId == clientUserId)
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

        private async Task ValidateBookingFormAsync(BookingCreateViewModel formModel)
        {
            if (!formModel.AutoServiceId.HasValue)
            {
                if (!ModelState.TryGetValue(nameof(formModel.AutoServiceId), out var serviceState) || serviceState.Errors.Count == 0)
                {
                    ModelState.AddModelError(nameof(formModel.AutoServiceId), "Оберіть послугу.");
                }
            }
            else if (!await _dbContext.AutoServices.AnyAsync(autoService => autoService.Id == formModel.AutoServiceId.Value && autoService.IsActive))
            {
                ModelState.AddModelError(nameof(formModel.AutoServiceId), "Оберіть активну послугу.");
            }

            if (formModel.ScheduledAt <= DateTime.Now)
            {
                ModelState.AddModelError(nameof(formModel.ScheduledAt), "Оберіть майбутню дату та час.");
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
