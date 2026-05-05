using System.ComponentModel.DataAnnotations;
using AutoServiceBooking.Web.Validation;

namespace AutoServiceBooking.Web.ViewModels
{
    public class ProfileViewModel : IValidatableObject
    {
        [Required(ErrorMessage = UserValidationRules.FullNameRequiredMessage)]
        [StringLength(UserValidationRules.FullNameMaxLength, MinimumLength = UserValidationRules.FullNameMinLength, ErrorMessage = UserValidationRules.FullNameLengthMessage)]
        [RegularExpression(UserValidationRules.FullNameRegex, ErrorMessage = UserValidationRules.FullNameRegexMessage)]
        [Display(Name = "Ім'я та прізвище")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = UserValidationRules.PhoneRequiredMessage)]
        [RegularExpression(UserValidationRules.PhoneRegex, ErrorMessage = UserValidationRules.PhoneRegexMessage)]
        [StringLength(UserValidationRules.PhoneMaxLength, ErrorMessage = UserValidationRules.PhoneLengthMessage)]
        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Поточний пароль")]
        public string? CurrentPassword { get; set; }

        [StringLength(UserValidationRules.PasswordMaxLength, MinimumLength = UserValidationRules.PasswordMinLength, ErrorMessage = UserValidationRules.NewPasswordLengthMessage)]
        [RegularExpression(UserValidationRules.PasswordRegex, ErrorMessage = UserValidationRules.NewPasswordRegexMessage)]
        [DataType(DataType.Password)]
        [Display(Name = "Новий пароль")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = UserValidationRules.PasswordCompareMessage)]
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
