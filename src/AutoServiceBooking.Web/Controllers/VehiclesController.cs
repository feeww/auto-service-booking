using AutoServiceBooking.Web.Data;
using AutoServiceBooking.Web.Extensions;
using AutoServiceBooking.Web.Models;
using AutoServiceBooking.Web.Services.Exports;
using AutoServiceBooking.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBooking.Web.Controllers
{
    [Authorize]
    public class VehiclesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly IExportService _exportService;

        public VehiclesController(ApplicationDbContext dbContext, IExportService exportService)
        {
            _dbContext = dbContext;
            _exportService = exportService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string tab = "active")
        {
            int clientUserId = User.GetUserId();
            bool showArchived = tab == "archive";

            List<VehicleListItemViewModel> vehicles = await _dbContext.Vehicles
                .Where(vehicle => vehicle.ClientUserId == clientUserId)
                .Where(vehicle => vehicle.IsArchived == showArchived)
                .OrderByDescending(vehicle => vehicle.CreatedAt)
                .Select(vehicle => new VehicleListItemViewModel
                {
                    Id = vehicle.Id,
                    Make = vehicle.Make,
                    Model = vehicle.Model,
                    Year = vehicle.Year,
                    LicensePlate = vehicle.LicensePlate,
                    Mileage = vehicle.Mileage,
                    FuelType = vehicle.FuelType,
                    BookingsCount = vehicle.Bookings.Count,
                    IsArchived = vehicle.IsArchived
                })
                .ToListAsync();

            foreach (VehicleListItemViewModel vehicle in vehicles)
            {
                vehicle.FuelTypeName = vehicle.FuelType.GetDisplayName();
            }

            VehicleIndexViewModel model = new VehicleIndexViewModel
            {
                Vehicles = vehicles,
                ActiveCount = await _dbContext.Vehicles.CountAsync(vehicle => vehicle.ClientUserId == clientUserId && !vehicle.IsArchived),
                ArchivedCount = await _dbContext.Vehicles.CountAsync(vehicle => vehicle.ClientUserId == clientUserId && vehicle.IsArchived),
                ShowArchived = showArchived
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            VehicleFormViewModel model = new VehicleFormViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleFormViewModel formModel)
        {
            if (!ModelState.IsValid)
            {
                return View(formModel);
            }

            int clientUserId = User.GetUserId();
            Vehicle vehicle = new Vehicle(
                clientUserId,
                formModel.Make,
                formModel.Model,
                formModel.Year,
                formModel.LicensePlate,
                formModel.Mileage,
                formModel.FuelType);

            _dbContext.Vehicles.Add(vehicle);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Автомобіль додано.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Vehicle? vehicle = await FindOwnVehicleAsync(id);

            if (vehicle == null)
            {
                return NotFound();
            }

            VehicleFormViewModel model = new VehicleFormViewModel
            {
                Id = vehicle.Id,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                LicensePlate = vehicle.LicensePlate,
                Mileage = vehicle.Mileage,
                FuelType = vehicle.FuelType
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VehicleFormViewModel formModel)
        {
            if (id != formModel.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(formModel);
            }

            Vehicle? vehicle = await FindOwnVehicleAsync(id);

            if (vehicle == null)
            {
                return NotFound();
            }

            vehicle.Update(formModel.Make, formModel.Model, formModel.Year, formModel.LicensePlate, formModel.Mileage, formModel.FuelType);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Автомобіль оновлено.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            Vehicle? vehicle = await FindOwnVehicleAsync(id);

            if (vehicle == null)
            {
                return NotFound();
            }

            VehicleListItemViewModel model = new VehicleListItemViewModel
            {
                Id = vehicle.Id,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                LicensePlate = vehicle.LicensePlate,
                Mileage = vehicle.Mileage,
                FuelType = vehicle.FuelType,
                FuelTypeName = vehicle.FuelType.GetDisplayName(),
                BookingsCount = await _dbContext.Bookings.CountAsync(booking => booking.VehicleId == vehicle.Id),
                IsArchived = vehicle.IsArchived
            };

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Vehicle? vehicle = await FindOwnVehicleAsync(id);

            if (vehicle == null)
            {
                return NotFound();
            }

            bool hasBookings = await _dbContext.Bookings.AnyAsync(booking => booking.VehicleId == vehicle.Id);

            if (hasBookings)
            {
                vehicle.Archive();
                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Автомобіль перенесено в архів. Старі записи залишились в історії.";
                return RedirectToAction(nameof(Index), new { tab = "archive" });
            }

            _dbContext.Vehicles.Remove(vehicle);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Автомобіль видалено.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            Vehicle? vehicle = await FindOwnVehicleAsync(id);

            if (vehicle == null)
            {
                return NotFound();
            }

            vehicle.Restore();
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Автомобіль повернено до гаража.";
            return RedirectToAction(nameof(Index), new { tab = "active" });
        }

        [HttpGet]
        public async Task<IActionResult> HistoryPdf(int id)
        {
            Vehicle? vehicle = await FindOwnVehicleAsync(id);

            if (vehicle == null)
            {
                return NotFound();
            }

            List<Booking> bookings = await GetVehicleHistoryBookingsAsync(vehicle.Id);
            byte[] pdf = _exportService.CreateVehicleHistoryPdf(vehicle, bookings);

            return File(pdf, "application/pdf", $"drivefix-{CreateSafeFileName(vehicle.LicensePlate)}-history.pdf");
        }

        private async Task<List<Booking>> GetVehicleHistoryBookingsAsync(int vehicleId)
        {
            return await _dbContext.Bookings
                .Include(booking => booking.AutoService)
                .Where(booking => booking.VehicleId == vehicleId && booking.Status == BookingStatus.Completed)
                .OrderByDescending(booking => booking.ScheduledAt)
                .ToListAsync();
        }

        private async Task<Vehicle?> FindOwnVehicleAsync(int id)
        {
            int clientUserId = User.GetUserId();

            return await _dbContext.Vehicles
                .FirstOrDefaultAsync(vehicle => vehicle.Id == id && vehicle.ClientUserId == clientUserId);
        }

        private static string CreateSafeFileName(string value)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string safeValue = new string(value.Select(character => invalidChars.Contains(character) ? '-' : character).ToArray());
            return string.IsNullOrWhiteSpace(safeValue) ? "vehicle" : safeValue.ToLowerInvariant();
        }

    }
}
