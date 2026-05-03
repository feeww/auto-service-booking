using System.ComponentModel.DataAnnotations;
using AutoServiceBooking.Web.Models;

namespace AutoServiceBooking.Web.ViewModels
{
    public class VehicleFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Вкажіть марку автомобіля")]
        [StringLength(60, ErrorMessage = "Марка може містити максимум 60 символів")]
        [Display(Name = "Марка")]
        public string Make { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть модель автомобіля")]
        [StringLength(60, ErrorMessage = "Модель може містити максимум 60 символів")]
        [Display(Name = "Модель")]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2100, ErrorMessage = "Вкажіть коректний рік випуску")]
        [Display(Name = "Рік випуску")]
        public int Year { get; set; } = DateTime.UtcNow.Year;

        [Required(ErrorMessage = "Вкажіть номерний знак")]
        [StringLength(20, ErrorMessage = "Номерний знак може містити максимум 20 символів")]
        [Display(Name = "Номерний знак")]
        public string LicensePlate { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Пробіг не може бути від'ємним")]
        [Display(Name = "Пробіг, км")]
        public int Mileage { get; set; }

        [Display(Name = "Тип пального")]
        public VehicleFuelType FuelType { get; set; }
    }
}
