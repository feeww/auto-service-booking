using AutoServiceBooking.Web.ViewModels;

namespace AutoServiceBooking.Web.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterViewModel model);

        Task<AuthResult> LoginAsync(LoginViewModel model);
    }
}
