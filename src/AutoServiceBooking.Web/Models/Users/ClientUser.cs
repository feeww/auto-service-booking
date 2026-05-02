using AutoServiceBooking.Web.Models;

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

        public ICollection<Vehicle> Vehicles { get; private set; } = new List<Vehicle>();

        public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();
    }
}
