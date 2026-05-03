using AutoServiceBooking.Web.Data;
using AutoServiceBooking.Web.Extensions;
using AutoServiceBooking.Web.Models;
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

        public VehiclesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int clientUserId = User.GetUserId();

            List<VehicleListItemViewModel> vehicles = await _dbContext.Vehicles
                .Where(vehicle => vehicle.ClientUserId == clientUserId)
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
                    BookingsCount = vehicle.Bookings.Count
                })
                .ToListAsync();

            foreach (VehicleListItemViewModel vehicle in vehicles)
            {
                vehicle.FuelTypeName = vehicle.FuelType.GetDisplayName();
            }

            return View(vehicles);
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
                BookingsCount = await _dbContext.Bookings.CountAsync(booking => booking.VehicleId == vehicle.Id)
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
                TempData["ErrorMessage"] = "Неможливо видалити автомобіль, бо для нього вже є записи на сервіс.";
                return RedirectToAction(nameof(Index));
            }

            _dbContext.Vehicles.Remove(vehicle);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Автомобіль видалено.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<Vehicle?> FindOwnVehicleAsync(int id)
        {
            int clientUserId = User.GetUserId();

            return await _dbContext.Vehicles
                .FirstOrDefaultAsync(vehicle => vehicle.Id == id && vehicle.ClientUserId == clientUserId);
        }

    }
}
