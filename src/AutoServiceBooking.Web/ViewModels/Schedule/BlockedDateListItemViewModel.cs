namespace AutoServiceBooking.Web.ViewModels
{
    public class BlockedDateListItemViewModel
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
