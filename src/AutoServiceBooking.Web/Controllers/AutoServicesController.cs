using AutoServiceBooking.Web.Data;
using AutoServiceBooking.Web.Models;
using AutoServiceBooking.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBooking.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AutoServicesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public AutoServicesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<AutoService> services = await _dbContext.AutoServices
                .Include(autoService => autoService.Bookings)
                .OrderBy(autoService => autoService.Name == "Інше")
                .ThenBy(autoService => autoService.Id)
                .ToListAsync();

            List<AutoServiceListItemViewModel> model = services
                .Select((autoService, index) => new AutoServiceListItemViewModel
                {
                    Id = autoService.Id,
                    DisplayNumber = index + 1,
                    Name = autoService.Name,
                    Description = autoService.Description,
                    Price = autoService.Price,
                    DurationMinutes = autoService.DurationMinutes,
                    IsActive = autoService.IsActive,
                    BookingsCount = autoService.Bookings.Count
                })
                .ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new AutoServiceFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AutoServiceFormViewModel formModel)
        {
            await ValidateServiceNameAsync(formModel);

            if (!ModelState.IsValid)
            {
                return View(formModel);
            }

            try
            {
                AutoService autoService = new AutoService(
                    formModel.Name,
                    formModel.Description,
                    formModel.Price,
                    formModel.DurationMinutes);

                _dbContext.AutoServices.Add(autoService);
                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Послугу додано.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                return View(formModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            AutoService? autoService = await _dbContext.AutoServices.FirstOrDefaultAsync(service => service.Id == id);

            if (autoService == null)
            {
                return NotFound();
            }

            AutoServiceFormViewModel model = new AutoServiceFormViewModel
            {
                Id = autoService.Id,
                Name = autoService.Name,
                Description = autoService.Description,
                Price = autoService.Price,
                DurationMinutes = autoService.DurationMinutes
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AutoServiceFormViewModel formModel)
        {
            if (id != formModel.Id)
            {
                return BadRequest();
            }

            await ValidateServiceNameAsync(formModel);

            if (!ModelState.IsValid)
            {
                return View(formModel);
            }

            AutoService? autoService = await _dbContext.AutoServices.FirstOrDefaultAsync(service => service.Id == id);

            if (autoService == null)
            {
                return NotFound();
            }

            try
            {
                autoService.Update(formModel.Name, formModel.Description, formModel.Price, formModel.DurationMinutes);
                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Послугу оновлено.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                return View(formModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            AutoService? autoService = await _dbContext.AutoServices.FirstOrDefaultAsync(service => service.Id == id);

            if (autoService == null)
            {
                return NotFound();
            }

            if (autoService.IsActive)
            {
                autoService.Deactivate();
                TempData["SuccessMessage"] = "Послугу приховано з форми запису.";
            }
            else
            {
                autoService.Activate();
                TempData["SuccessMessage"] = "Послугу знову доступна у формі запису.";
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            AutoService? autoService = await _dbContext.AutoServices.FirstOrDefaultAsync(service => service.Id == id);

            if (autoService == null)
            {
                return NotFound();
            }

            AutoServiceListItemViewModel model = new AutoServiceListItemViewModel
            {
                Id = autoService.Id,
                Name = autoService.Name,
                Description = autoService.Description,
                Price = autoService.Price,
                DurationMinutes = autoService.DurationMinutes,
                IsActive = autoService.IsActive,
                BookingsCount = await _dbContext.Bookings.CountAsync(booking => booking.AutoServiceId == autoService.Id)
            };

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            AutoService? autoService = await _dbContext.AutoServices.FirstOrDefaultAsync(service => service.Id == id);

            if (autoService == null)
            {
                return NotFound();
            }

            bool hasBookings = await _dbContext.Bookings.AnyAsync(booking => booking.AutoServiceId == autoService.Id);

            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Неможливо видалити послугу, бо для неї вже є записи. Приховайте її замість видалення.";
                return RedirectToAction(nameof(Index));
            }

            _dbContext.AutoServices.Remove(autoService);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Послугу видалено.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateServiceNameAsync(AutoServiceFormViewModel formModel)
        {
            string normalizedName = formModel.Name.Trim().ToLowerInvariant();
            bool nameExists = await _dbContext.AutoServices
                .AnyAsync(autoService => autoService.Id != formModel.Id && autoService.Name.ToLower() == normalizedName);

            if (nameExists)
            {
                ModelState.AddModelError(nameof(formModel.Name), "Послуга з такою назвою вже існує.");
            }
        }
    }
}
