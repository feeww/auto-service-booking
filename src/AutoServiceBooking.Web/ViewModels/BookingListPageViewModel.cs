using AutoServiceBooking.Web.Models;

namespace AutoServiceBooking.Web.ViewModels
{
    public class BookingListPageViewModel
    {
        public List<BookingListItemViewModel> Bookings { get; set; } = new();
        public string? Search { get; set; }
        public BookingStatus? Status { get; set; }
        public List<BookingStatusOptionViewModel> StatusOptions { get; set; } = new();
        public bool HasFilters => !string.IsNullOrWhiteSpace(Search) || Status.HasValue;
    }

    public class BookingStatusOptionViewModel
    {
        public string Value { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;
    }
}
