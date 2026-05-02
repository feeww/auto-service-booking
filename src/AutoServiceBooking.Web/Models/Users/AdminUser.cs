namespace AutoServiceBooking.Web.Models.Users
{
    public class AdminUser : AppUser
    {
        protected AdminUser()
        {
            Role = UserRole.Admin;
        }

        public AdminUser(string fullName, string email, string passwordHash, string? phoneNumber): base(fullName, email, passwordHash, phoneNumber)
        {
            Role = UserRole.Admin;
        }
    }
}
