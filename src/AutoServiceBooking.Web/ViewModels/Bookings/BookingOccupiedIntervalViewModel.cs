namespace AutoServiceBooking.Web.ViewModels
{
    public class BookingOccupiedIntervalViewModel
    {
        public int BookingId { get; set; }

        public string DateValue { get; set; } = string.Empty;

        public string StartTime { get; set; } = string.Empty;

        public string EndTime { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }
}
