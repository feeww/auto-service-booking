namespace AutoServiceBooking.Web.ViewModels
{
    public class VehicleIndexViewModel
    {
        public List<VehicleListItemViewModel> Vehicles { get; set; } = new();

        public int ActiveCount { get; set; }

        public int ArchivedCount { get; set; }

        public bool ShowArchived { get; set; }
    }
}
