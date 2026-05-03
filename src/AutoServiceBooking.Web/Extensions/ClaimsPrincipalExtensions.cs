using System.Security.Claims;

namespace AutoServiceBooking.Web.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int parsedUserId))
            {
                throw new InvalidOperationException("Не вдалося визначити поточного користувача.");
            }

            return parsedUserId;
        }
    }
}
