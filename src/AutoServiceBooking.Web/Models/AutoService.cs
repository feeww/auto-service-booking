using System.ComponentModel.DataAnnotations;

namespace AutoServiceBooking.Web.Models
{
    public class AutoService
    {
        protected AutoService()
        {
        }

        public AutoService(string name, string description, decimal price, int durationMinutes)
        {
            Validate(name, description, price, durationMinutes);

            Name = name.Trim();
            Description = description.Trim();
            Price = price;
            DurationMinutes = durationMinutes;
            IsActive = true;
        }

        public int Id { get; private set; }

        [Required]
        [StringLength(100)]
        public string Name { get; private set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; private set; } = string.Empty;

        public decimal Price { get; private set; }

        public int DurationMinutes { get; private set; }

        public bool IsActive { get; private set; } = true;

        public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();

        public void Update(string name, string description, decimal price, int durationMinutes)
        {
            Validate(name, description, price, durationMinutes);

            Name = name.Trim();
            Description = description.Trim();
            Price = price;
            DurationMinutes = durationMinutes;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        private static void Validate(string name, string description, decimal price, int durationMinutes)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Назва послуги обов'язкова.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Опис послуги обов'язковий.", nameof(description));
            }

            if (price < 0)
            {
                throw new ArgumentException("Ціна не може бути від'ємною.", nameof(price));
            }

            if (durationMinutes <= 0)
            {
                throw new ArgumentException("Тривалість має бути більшою за нуль.", nameof(durationMinutes));
            }
        }
    }
}
