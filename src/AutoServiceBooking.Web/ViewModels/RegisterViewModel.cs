using System.ComponentModel.DataAnnotations;

namespace AutoServiceBooking.Web.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Вкажіть ім'я")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ім'я має містити від 2 до 100 символів")]
        [RegularExpression(@"^[A-Za-zА-Яа-яІіЇїЄєҐґ'’\-\s]+$", ErrorMessage = "Ім'я може містити тільки літери, пробіли, апостроф або дефіс")]
        [Display(Name = "Ім'я та прізвище")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть email")]
        [EmailAddress(ErrorMessage = "Некоректний email")]
        [StringLength(100, ErrorMessage = "Email може містити максимум 100 символів")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть телефон")]
        [RegularExpression(@"^(\+?380|0)[\s\-]?\d{2}[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}$", ErrorMessage = "Вкажіть телефон у форматі +380XXXXXXXXX або 0XXXXXXXXX")]
        [StringLength(30, ErrorMessage = "Телефон може містити максимум 30 символів")]
        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть пароль")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль має містити від 8 до 100 символів")]
        [RegularExpression(@"^(?=.*[A-Za-zА-Яа-яІіЇїЄєҐґ])(?=.*\d).+$", ErrorMessage = "Пароль має містити хоча б одну літеру та одну цифру")]
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
