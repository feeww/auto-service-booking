namespace AutoServiceBooking.Web.ViewModels
{
    public class BookingListItemViewModel
    {
        public int Id { get; set; }

        public string AutoServiceName { get; set; } = string.Empty;

        public decimal AutoServicePrice { get; set; }

        public int AutoServiceDurationMinutes { get; set; }

        public DateTime ScheduledAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string StatusCssClass { get; set; } = string.Empty;

        public bool IsClosedWithoutEstimate { get; set; }

        public string VehicleTitle { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public string? CustomerEmail { get; set; }

        public string? ProblemDescription { get; set; }

        public decimal? FinalPrice { get; set; }

        public decimal? EstimatedPrice { get; set; }

        public int? EstimatedDurationMinutes { get; set; }

        public int RescheduleDurationMinutes => EstimatedDurationMinutes ?? AutoServiceDurationMinutes;

        public string? AdminComment { get; set; }

        public bool CanCancel { get; set; }

        public bool CanConfirm { get; set; }

        public bool CanReject { get; set; }

        public bool CanReschedule { get; set; }

        public bool CanStartWork { get; set; }

        public bool CanComplete { get; set; }

    }
}
