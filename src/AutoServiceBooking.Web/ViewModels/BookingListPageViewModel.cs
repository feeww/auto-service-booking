using AutoServiceBooking.Web.Models;

namespace AutoServiceBooking.Web.ViewModels
{
    public class BookingListPageViewModel
    {
        public List<BookingListItemViewModel> Bookings { get; set; } = new();
        public string? Search { get; set; }
        public BookingStatus? Status { get; set; }
        public string Sort { get; set; } = "nearest";
        public List<BookingStatusOptionViewModel> StatusOptions { get; set; } = new();
        public List<BookingSortOptionViewModel> SortOptions { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasFilters => !string.IsNullOrWhiteSpace(Search) || Status.HasValue;
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public int FirstItemNumber => TotalItems == 0 ? 0 : ((Page - 1) * PageSize) + 1;
        public int LastItemNumber => Math.Min(Page * PageSize, TotalItems);
    }

    public class BookingStatusOptionViewModel
    {
        public string Value { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;
    }

    public class BookingSortOptionViewModel
    {
        public string Value { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;
    }
}
