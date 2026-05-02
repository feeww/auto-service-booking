using System.ComponentModel.DataAnnotations;

namespace AutoServiceBooking.Web.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Вкажіть ім'я")]
        [StringLength(100)]
        [Display(Name = "Ім'я та прізвище")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть email")]
        [EmailAddress(ErrorMessage = "Некоректний email")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Некоректний номер телефону")]
        [StringLength(30)]
        [Display(Name = "Телефон")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Вкажіть пароль")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль має містити мінімум 6 символів")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Повторіть пароль")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Паролі не співпадають")]
        [Display(Name = "Повторіть пароль")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
