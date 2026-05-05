namespace AutoServiceBooking.Web.ViewModels
{
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
}
