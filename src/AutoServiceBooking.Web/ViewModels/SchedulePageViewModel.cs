using System.ComponentModel.DataAnnotations;

namespace AutoServiceBooking.Web.ViewModels
{
    public class SchedulePageViewModel
    {
        public DateTime SelectedDate { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public BlockDateFormViewModel NewBlockedDate { get; set; } = new BlockDateFormViewModel();

        public List<ScheduleDayViewModel> Days { get; set; } = new List<ScheduleDayViewModel>();

        public List<ScheduleBookingViewModel> Bookings { get; set; } = new List<ScheduleBookingViewModel>();

        public List<BlockedDateListItemViewModel> BlockedDates { get; set; } = new List<BlockedDateListItemViewModel>();
    }

    public class BlockDateFormViewModel
    {
        [Required(ErrorMessage = "Оберіть дату")]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; } = DateTime.Today.AddDays(1);

        [StringLength(200, ErrorMessage = "Причина може містити максимум 200 символів")]
        [Display(Name = "Причина")]
        public string? Reason { get; set; }
    }

    public class ScheduleDayViewModel
    {
        public DateTime Date { get; set; }

        public int BookingCount { get; set; }

        public bool IsSelected { get; set; }

        public bool IsBlocked { get; set; }

        public string? BlockReason { get; set; }
    }

    public class ScheduleBookingViewModel
    {
        public int Id { get; set; }

        public DateTime ScheduledAt { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public string AutoServiceName { get; set; } = string.Empty;

        public string VehicleTitle { get; set; } = string.Empty;

        public string StatusName { get; set; } = string.Empty;

        public string StatusCssClass { get; set; } = string.Empty;
    }

    public class BlockedDateListItemViewModel
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
