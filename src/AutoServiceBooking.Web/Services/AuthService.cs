using AutoServiceBooking.Web.Data;
using AutoServiceBooking.Web.Models.Users;
using AutoServiceBooking.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBooking.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthResult> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                string email = NormalizeEmail(model.Email);
                bool emailExists = await _context.Users.AnyAsync(user => user.Email == email);

                if (emailExists)
                {
                    return CreateError("Користувач з таким email вже існує", nameof(model.Email));
                }

                string passwordHash = _passwordHasher.HashPassword(model.Password);
                ClientUser user = new ClientUser(model.FullName.Trim(), email, passwordHash, GetPhoneNumber(model.PhoneNumber));

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return CreateSuccess(user);
            }
            catch (DbUpdateException)
            {
                return CreateError("Не вдалося зберегти користувача. Спробуйте ще раз.");
            }
        }

        public async Task<AuthResult> LoginAsync(LoginViewModel model)
        {
            try
            {
                string email = NormalizeEmail(model.Email);
                AppUser? user = await _context.Users.FirstOrDefaultAsync(user => user.Email == email);

                if (user == null || !_passwordHasher.VerifyPassword(model.Password, user.PasswordHash))
                {
                    return CreateError("Невірний email або пароль");
                }

                return CreateSuccess(user);
            }
            catch (Exception)
            {
                return CreateError("Не вдалося виконати вхід. Спробуйте ще раз.");
            }
        }

        private string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        private string? GetPhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return null;
            }

            return phoneNumber.Trim();
        }

        private AuthResult CreateSuccess(AppUser user)
        {
            AuthResult result = new AuthResult();
            result.IsSuccess = true;
            result.User = user;

            return result;
        }

        private AuthResult CreateError(string message, string? fieldName = null)
        {
            AuthResult result = new AuthResult();
            result.IsSuccess = false;
            result.ErrorMessage = message;
            result.FieldName = fieldName;

            return result;
        }
    }
}
