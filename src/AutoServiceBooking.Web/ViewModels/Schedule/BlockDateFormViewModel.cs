using System.ComponentModel.DataAnnotations;

namespace AutoServiceBooking.Web.ViewModels
{
    public class BlockDateFormViewModel
    {
        [Required(ErrorMessage = "Оберіть дату")]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; } = DateTime.Today.AddDays(1);

        [StringLength(200, ErrorMessage = "Причина може містити максимум 200 символів")]
        [Display(Name = "Причина")]
        public string? Reason { get; set; }
    }
}
