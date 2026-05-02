using System.ComponentModel.DataAnnotations;

namespace AutoServiceBooking.Web.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Вкажіть email")]
        [EmailAddress(ErrorMessage = "Некоректний email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;
    }
}
