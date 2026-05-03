using System.Security.Claims;
using AutoServiceBooking.Web.Data;
using AutoServiceBooking.Web.Extensions;
using AutoServiceBooking.Web.Models.Users;
using AutoServiceBooking.Web.Services;
using AutoServiceBooking.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBooking.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;

        public AccountController(IAuthService authService, ApplicationDbContext dbContext, IPasswordHasher passwordHasher)
        {
            _authService = authService;
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            AuthResult result = await _authService.RegisterAsync(model);

            if (!result.IsSuccess || result.User == null)
            {
                AddAuthError(result);
                return View(model);
            }

            await SignInAsync(result.User);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            AuthResult result = await _authService.LoginAsync(model);

            if (!result.IsSuccess || result.User == null)
            {
                AddAuthError(result);
                return View(model);
            }

            await SignInAsync(result.User);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            AppUser? user = await FindCurrentUserAsync();

            if (user == null)
            {
                return NotFound();
            }

            ProfileViewModel model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            AppUser? user = await FindCurrentUserAsync();

            if (user == null)
            {
                return NotFound();
            }

            model.Email = user.Email;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                user.UpdateProfile(model.FullName, model.PhoneNumber);

                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    if (string.IsNullOrWhiteSpace(model.CurrentPassword) || !_passwordHasher.VerifyPassword(model.CurrentPassword, user.PasswordHash))
                    {
                        ModelState.AddModelError(nameof(model.CurrentPassword), "Поточний пароль неправильний.");
                        return View(model);
                    }

                    user.UpdatePassword(_passwordHasher.HashPassword(model.NewPassword));
                }

                await _dbContext.SaveChangesAsync();
                await SignInAsync(user);

                TempData["SuccessMessage"] = "Профіль оновлено.";
                return RedirectToAction(nameof(Profile));
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                return View(model);
            }
        }

        private void AddAuthError(AuthResult result)
        {
            string fieldName = result.FieldName ?? string.Empty;
            string message = result.ErrorMessage ?? "Сталася помилка. Спробуйте ще раз.";

            ModelState.AddModelError(fieldName, message);
        }

        private async Task SignInAsync(AppUser user)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.FullName));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        private async Task<AppUser?> FindCurrentUserAsync()
        {
            int userId = User.GetUserId();
            return await _dbContext.Users.FirstOrDefaultAsync(user => user.Id == userId);
        }
    }
}
