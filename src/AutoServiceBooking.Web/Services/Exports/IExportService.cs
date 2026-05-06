using AutoServiceBooking.Web.Models;

namespace AutoServiceBooking.Web.Services.Exports
{
    public interface IExportService
    {
        byte[] CreateBookingActPdf(Booking booking);

        byte[] CreateVehicleHistoryPdf(Vehicle vehicle, IReadOnlyList<Booking> bookings);
    }
}
