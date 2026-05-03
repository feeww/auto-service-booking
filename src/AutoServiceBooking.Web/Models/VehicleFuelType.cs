using System.ComponentModel.DataAnnotations;

namespace AutoServiceBooking.Web.Models
{
    public enum VehicleFuelType
    {
        [Display(Name = "Бензин")]
        Petrol,

        [Display(Name = "Дизель")]
        Diesel,

        [Display(Name = "Гібрид")]
        Hybrid,

        [Display(Name = "Електро")]
        Electric
    }
}
