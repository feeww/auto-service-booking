namespace AutoServiceBooking.Web.ViewModels
{
    public class AutoServiceListItemViewModel
    {
        public int Id { get; set; }

        public int DisplayNumber { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int DurationMinutes { get; set; }

        public bool IsActive { get; set; }

        public int BookingsCount { get; set; }
    }
}
