using System.Diagnostics;
using AutoServiceBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutoServiceBooking.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            ErrorViewModel model = new ErrorViewModel();
            model.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            return View(model);
        }
    }
}
