namespace AutoServiceBooking.Web.ViewModels
{
    public class ScheduleDayViewModel
    {
        public DateTime Date { get; set; }

        public int BookingCount { get; set; }

        public bool IsSelected { get; set; }

        public bool IsBlocked { get; set; }

        public string? BlockReason { get; set; }
    }
}
