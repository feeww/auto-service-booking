using System.ComponentModel.DataAnnotations;

namespace AutoServiceBooking.Web.ViewModels
{
    public class ProfileViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Вкажіть ім'я")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ім'я має містити від 2 до 100 символів")]
        [RegularExpression(@"^[A-Za-zА-Яа-яІіЇїЄєҐґ'’\-\s]+$", ErrorMessage = "Ім'я може містити тільки літери, пробіли, апостроф або дефіс")]
        [Display(Name = "Ім'я та прізвище")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть телефон")]
        [RegularExpression(@"^(\+?380|0)[\s\-]?\d{2}[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}$", ErrorMessage = "Вкажіть телефон у форматі +380XXXXXXXXX або 0XXXXXXXXX")]
        [StringLength(30, ErrorMessage = "Телефон може містити максимум 30 символів")]
        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Поточний пароль")]
        public string? CurrentPassword { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "Новий пароль має містити від 8 до 100 символів")]
        [RegularExpression(@"^(?=.*[A-Za-zА-Яа-яІіЇїЄєҐґ])(?=.*\d).+$", ErrorMessage = "Новий пароль має містити хоча б одну літеру та одну цифру")]
        [DataType(DataType.Password)]
        [Display(Name = "Новий пароль")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Паролі не співпадають")]
        [Display(Name = "Повторіть новий пароль")]
        public string? ConfirmNewPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            bool wantsToChangePassword =
                !string.IsNullOrWhiteSpace(CurrentPassword) ||
                !string.IsNullOrWhiteSpace(NewPassword) ||
                !string.IsNullOrWhiteSpace(ConfirmNewPassword);

            if (!wantsToChangePassword)
            {
                yield break;
            }

            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                yield return new ValidationResult("Вкажіть поточний пароль", new[] { nameof(CurrentPassword) });
            }

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                yield return new ValidationResult("Вкажіть новий пароль", new[] { nameof(NewPassword) });
            }

            if (string.IsNullOrWhiteSpace(ConfirmNewPassword))
            {
                yield return new ValidationResult("Повторіть новий пароль", new[] { nameof(ConfirmNewPassword) });
            }
        }
    }
}
