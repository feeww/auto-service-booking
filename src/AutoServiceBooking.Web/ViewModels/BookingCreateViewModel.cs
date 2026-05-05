using System.ComponentModel.DataAnnotations;
using AutoServiceBooking.Web.Models;
using AutoServiceBooking.Web.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AutoServiceBooking.Web.ViewModels
{
    public class BookingCreateViewModel
    {
        [Required(ErrorMessage = "Оберіть послугу")]
        [Range(1, int.MaxValue, ErrorMessage = "Оберіть послугу")]
        [Display(Name = "Послуга")]
        public int? AutoServiceId { get; set; }

        [Required(ErrorMessage = "Оберіть дату та час")]
        [Display(Name = "Дата та час")]
        public DateTime ScheduledAt { get; set; } = DateTime.Now.AddDays(1).Date.AddHours(10);

        [StringLength(1000, ErrorMessage = "Опис може містити максимум 1000 символів")]
        [Display(Name = "Опис проблеми")]
        public string? ProblemDescription { get; set; }

        [Required(ErrorMessage = "Вкажіть ім'я")]
        [StringLength(100, ErrorMessage = "Ім'я може містити максимум 100 символів")]
        [Display(Name = "Ім'я та прізвище")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть телефон")]
        [RegularExpression(UserValidationRules.PhoneRegex, ErrorMessage = UserValidationRules.PhoneRegexMessage)]
        [StringLength(UserValidationRules.PhoneMaxLength, ErrorMessage = UserValidationRules.PhoneLengthMessage)]
        [Display(Name = "Телефон")]
        public string CustomerPhone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = UserValidationRules.EmailInvalidMessage)]
        [StringLength(UserValidationRules.EmailMaxLength, ErrorMessage = UserValidationRules.EmailLengthMessage)]
        [Display(Name = "Email")]
        public string? CustomerEmail { get; set; }

        [Display(Name = "Збережене авто")]
        public int? SelectedVehicleId { get; set; }

        [Display(Name = "Марка")]
        [StringLength(60, ErrorMessage = "Марка може містити максимум 60 символів")]
        public string? VehicleMake { get; set; }

        [Display(Name = "Модель")]
        [StringLength(60, ErrorMessage = "Модель може містити максимум 60 символів")]
        public string? VehicleModel { get; set; }

        [Display(Name = "Рік випуску")]
        [Range(1900, 2100, ErrorMessage = "Вкажіть коректний рік випуску")]
        public int? VehicleYear { get; set; }

        [Display(Name = "Номерний знак")]
        [StringLength(20, ErrorMessage = "Номерний знак може містити максимум 20 символів")]
        public string? VehicleLicensePlate { get; set; }

        [Display(Name = "Пробіг, км")]
        [Range(0, int.MaxValue, ErrorMessage = "Пробіг не може бути від'ємним")]
        public int? VehicleMileage { get; set; }

        [Display(Name = "Тип пального")]
        public VehicleFuelType VehicleFuelType { get; set; }

        public bool IsAuthenticatedUser { get; set; }

        public DateTime MinScheduledAt { get; set; } = DateTime.Now.AddMinutes(30);

        public int WorkDayStartHour { get; set; } = 9;

        public int WorkDayEndHour { get; set; } = 19;

        public int SlotStepMinutes { get; set; } = 30;

        public List<SelectListItem> AutoServiceOptions { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> SavedVehicleOptions { get; set; } = new List<SelectListItem>();

        public List<SavedVehicleBookingViewModel> SavedVehicles { get; set; } = new List<SavedVehicleBookingViewModel>();

        public List<BookingBlockedDateViewModel> BlockedDates { get; set; } = new List<BookingBlockedDateViewModel>();

        public List<BookingServiceDurationViewModel> ServiceDurations { get; set; } = new List<BookingServiceDurationViewModel>();

        public List<BookingOccupiedIntervalViewModel> OccupiedIntervals { get; set; } = new List<BookingOccupiedIntervalViewModel>();
    }

    public class SavedVehicleBookingViewModel
    {
        public int Id { get; set; }

        public string Make { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        public string LicensePlate { get; set; } = string.Empty;

        public int Mileage { get; set; }

        public string FuelTypeName { get; set; } = string.Empty;
    }

    public class BookingBlockedDateViewModel
    {
        public string DateValue { get; set; } = string.Empty;

        public string DisplayDate { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
    }

    public class BookingServiceDurationViewModel
    {
        public int ServiceId { get; set; }

        public int DurationMinutes { get; set; }
    }

    public class BookingOccupiedIntervalViewModel
    {
        public int BookingId { get; set; }

        public string DateValue { get; set; } = string.Empty;

        public string StartTime { get; set; } = string.Empty;

        public string EndTime { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }
}
