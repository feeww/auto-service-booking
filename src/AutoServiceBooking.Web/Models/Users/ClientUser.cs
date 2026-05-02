namespace AutoServiceBooking.Web.Models.Users
{
    public class ClientUser : AppUser
    {
        protected ClientUser()
        {
            Role = UserRole.Client;
        }

        public ClientUser(string fullName, string email, string passwordHash, string? phoneNumber): base(fullName, email, passwordHash, phoneNumber)
        {
            Role = UserRole.Client;
        }
    }
}
