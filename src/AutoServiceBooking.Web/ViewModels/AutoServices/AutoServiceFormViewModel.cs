using System.ComponentModel.DataAnnotations;

namespace AutoServiceBooking.Web.ViewModels
{
    public class AutoServiceFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Вкажіть назву послуги")]
        [StringLength(100, ErrorMessage = "Назва може містити максимум 100 символів")]
        [Display(Name = "Назва")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть опис послуги")]
        [StringLength(500, ErrorMessage = "Опис може містити максимум 500 символів")]
        [Display(Name = "Опис")]
        public string Description { get; set; } = string.Empty;

        [Range(0, 100000, ErrorMessage = "Ціна має бути від 0 до 100000 грн")]
        [Display(Name = "Орієнтовна ціна, грн")]
        public decimal Price { get; set; }

        [Range(1, 1440, ErrorMessage = "Тривалість має бути від 1 до 1440 хвилин")]
        [Display(Name = "Орієнтовна тривалість, хв")]
        public int DurationMinutes { get; set; } = 30;
    }
}
