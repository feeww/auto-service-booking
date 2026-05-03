using System.ComponentModel.DataAnnotations;

namespace AutoServiceBooking.Web.Models
{
    public enum BookingStatus
    {
        [Display(Name = "Очікує")]
        Pending,

        [Display(Name = "Підтверджено")]
        Confirmed,

        [Display(Name = "В роботі")]
        InProgress,

        [Display(Name = "Завершено")]
        Completed,

        [Display(Name = "Скасовано")]
        Cancelled,

        [Display(Name = "Відхилено")]
        Rejected
    }
}
