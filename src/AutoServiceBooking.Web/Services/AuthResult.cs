using AutoServiceBooking.Web.Models.Users;

namespace AutoServiceBooking.Web.Services
{
    public class AuthResult
    {
        public bool IsSuccess { get; set; }

        public string? ErrorMessage { get; set; }

        public string? FieldName { get; set; }

        public AppUser? User { get; set; }
    }
}
