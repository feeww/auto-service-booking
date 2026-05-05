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
}
