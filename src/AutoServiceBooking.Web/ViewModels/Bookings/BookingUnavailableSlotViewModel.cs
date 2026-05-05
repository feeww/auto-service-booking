namespace AutoServiceBooking.Web.ViewModels
{
    public class BookingUnavailableSlotViewModel
    {
        public int ServiceId { get; set; }

        public string DateValue { get; set; } = string.Empty;

        public string TimeValue { get; set; } = string.Empty;
    }
}
