using System.ComponentModel.DataAnnotations;
using AutoServiceBooking.Web.Validation;

namespace AutoServiceBooking.Web.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = UserValidationRules.EmailRequiredMessage)]
        [EmailAddress(ErrorMessage = UserValidationRules.EmailInvalidMessage)]
        [StringLength(UserValidationRules.EmailMaxLength, ErrorMessage = UserValidationRules.EmailLengthMessage)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = UserValidationRules.PasswordRequiredMessage)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;
    }
}
