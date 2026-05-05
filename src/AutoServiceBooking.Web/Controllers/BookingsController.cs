using AutoServiceBooking.Web.Data;
using AutoServiceBooking.Web.Services.Bookings;
using AutoServiceBooking.Web.Services.Scheduling;
using Microsoft.AspNetCore.Mvc;

namespace AutoServiceBooking.Web.Controllers
{
    public partial class BookingsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly IScheduleService _scheduleService;

        private readonly IBookingService _bookingService;

        public BookingsController(ApplicationDbContext dbContext, IScheduleService scheduleService, IBookingService bookingService)
        {
            _dbContext = dbContext;
            _scheduleService = scheduleService;
            _bookingService = bookingService;
        }
    }
}
