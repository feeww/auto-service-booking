using AutoServiceBooking.Web.Models;

namespace AutoServiceBooking.Web.ViewModels
{
    public class VehicleListItemViewModel
    {
        public int Id { get; set; }

        public string Make { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        public string LicensePlate { get; set; } = string.Empty;

        public int Mileage { get; set; }

        public VehicleFuelType FuelType { get; set; }

        public string FuelTypeName { get; set; } = string.Empty;

        public int BookingsCount { get; set; }
    }
}
