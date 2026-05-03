using AutoServiceBooking.Web.Data;
using Microsoft.AspNetCore.Mvc;

namespace AutoServiceBooking.Web.Controllers
{
    public partial class BookingsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public BookingsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
