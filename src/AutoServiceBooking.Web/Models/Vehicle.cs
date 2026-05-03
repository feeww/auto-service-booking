using System.ComponentModel.DataAnnotations;
using AutoServiceBooking.Web.Models.Users;

namespace AutoServiceBooking.Web.Models
{
    public class Vehicle
    {
        protected Vehicle()
        {
        }

        public Vehicle(int clientUserId, string make, string model, int year, string licensePlate, int mileage, VehicleFuelType fuelType)
        {
            Validate(make, model, year, licensePlate, mileage);

            ClientUserId = clientUserId;
            Make = make.Trim();
            Model = model.Trim();
            Year = year;
            LicensePlate = licensePlate.Trim().ToUpperInvariant();
            Mileage = mileage;
            FuelType = fuelType;
        }

        public int Id { get; private set; }

        public int ClientUserId { get; private set; }

        public ClientUser ClientUser { get; private set; } = null!;

        [Required]
        [StringLength(60)]
        public string Make { get; private set; } = string.Empty;

        [Required]
        [StringLength(60)]
        public string Model { get; private set; } = string.Empty;

        public int Year { get; private set; }

        [Required]
        [StringLength(20)]
        public string LicensePlate { get; private set; } = string.Empty;

        public int Mileage { get; private set; }

        public VehicleFuelType FuelType { get; private set; }

        public bool IsArchived { get; private set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();

        public void Update(string make, string model, int year, string licensePlate, int mileage, VehicleFuelType fuelType)
        {
            Validate(make, model, year, licensePlate, mileage);

            Make = make.Trim();
            Model = model.Trim();
            Year = year;
            LicensePlate = licensePlate.Trim().ToUpperInvariant();
            Mileage = mileage;
            FuelType = fuelType;
        }

        public void Archive()
        {
            IsArchived = true;
        }

        public void Restore()
        {
            IsArchived = false;
        }

        private static void Validate(string make, string model, int year, string licensePlate, int mileage)
        {
            if (string.IsNullOrWhiteSpace(make))
            {
                throw new ArgumentException("Марка автомобіля обов'язкова.", nameof(make));
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new ArgumentException("Модель автомобіля обов'язкова.", nameof(model));
            }

            if (year < 1900 || year > DateTime.UtcNow.Year + 1)
            {
                throw new ArgumentException("Рік випуску автомобіля некоректний.", nameof(year));
            }

            if (string.IsNullOrWhiteSpace(licensePlate))
            {
                throw new ArgumentException("Номерний знак обов'язковий.", nameof(licensePlate));
            }

            if (mileage < 0)
            {
                throw new ArgumentException("Пробіг не може бути від'ємним.", nameof(mileage));
            }
        }
    }
}
