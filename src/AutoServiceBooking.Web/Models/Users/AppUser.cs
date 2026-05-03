using System.ComponentModel.DataAnnotations;

namespace AutoServiceBooking.Web.Models.Users
{
    public abstract class AppUser
    {
        protected AppUser()
        {
        }

        protected AppUser(string fullName, string email, string passwordHash, string? phoneNumber)
        {
            FullName = fullName;
            Email = email;
            PasswordHash = passwordHash;
            PhoneNumber = phoneNumber;
        }

        public int Id { get; protected set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; private set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; private set; } = string.Empty;

        [Required]
        public string PasswordHash { get; private set; } = string.Empty;

        [Phone]
        [StringLength(30)]
        public string? PhoneNumber { get; private set; }

        public UserRole Role { get; protected set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public bool IsAdmin()
        {
            return Role == UserRole.Admin;
        }

        public void UpdateProfile(string fullName, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentException("Ім'я обов'язкове.", nameof(fullName));
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("Телефон обов'язковий.", nameof(phoneNumber));
            }

            FullName = fullName.Trim();
            PhoneNumber = phoneNumber.Trim();
        }

        public void UpdatePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
        }
    }
}
