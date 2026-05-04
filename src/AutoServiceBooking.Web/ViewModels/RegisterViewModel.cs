using System.ComponentModel.DataAnnotations;
using AutoServiceBooking.Web.Validation;

namespace AutoServiceBooking.Web.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = UserValidationRules.FullNameRequiredMessage)]
        [StringLength(UserValidationRules.FullNameMaxLength, MinimumLength = UserValidationRules.FullNameMinLength, ErrorMessage = UserValidationRules.FullNameLengthMessage)]
        [RegularExpression(UserValidationRules.FullNameRegex, ErrorMessage = UserValidationRules.FullNameRegexMessage)]
        [Display(Name = "Ім'я та прізвище")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = UserValidationRules.EmailRequiredMessage)]
        [EmailAddress(ErrorMessage = UserValidationRules.EmailInvalidMessage)]
        [StringLength(UserValidationRules.EmailMaxLength, ErrorMessage = UserValidationRules.EmailLengthMessage)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = UserValidationRules.PhoneRequiredMessage)]
        [RegularExpression(UserValidationRules.PhoneRegex, ErrorMessage = UserValidationRules.PhoneRegexMessage)]
        [StringLength(UserValidationRules.PhoneMaxLength, ErrorMessage = UserValidationRules.PhoneLengthMessage)]
        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = UserValidationRules.PasswordRequiredMessage)]
        [StringLength(UserValidationRules.PasswordMaxLength, MinimumLength = UserValidationRules.PasswordMinLength, ErrorMessage = UserValidationRules.PasswordLengthMessage)]
        [RegularExpression(UserValidationRules.PasswordRegex, ErrorMessage = UserValidationRules.PasswordRegexMessage)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Повторіть пароль")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = UserValidationRules.PasswordCompareMessage)]
        [Display(Name = "Повторіть пароль")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
